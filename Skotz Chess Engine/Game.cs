using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Timers;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class Game
    {
        public Board board;

        private Stopwatch stopwatch;
        private int evals;
        private bool cutoff;

        // TODO: this will not work when the engine is allowed to multi-thread since this assumes a singular user traversing through the tree
        private Dictionary<ulong, int> positions;

        // Transposition table
        private Move[] evaluations;
        private Board[] evaluations_positions;
        private const int evaluations_max = 256 * 256 * 8;
        private const ulong evaluations_max_mask = evaluations_max - 1;

        // Material hash table
        private int[] material_eval;
        private Board[] material_positions;
        private const int material_max = 256 * 256;
        private const ulong material_max_mask = material_max - 1;

        private int time_per_move;
        private int depth_per_move;

        public Game()
        {
            board = new Board();
            positions = new Dictionary<ulong, int>();

            evaluations = new Move[evaluations_max];
            evaluations_positions = new Board[evaluations_max];

            material_eval = new int[material_max];
            material_positions = new Board[material_max];
        }

        public void ResetBoard()
        {
            board = BoardGenerator.NewStandardSetup();
            positions = new Dictionary<ulong, int>();
        }

        public bool LoadBoard(string fen)
        {
            board = BoardGenerator.FromFEN(fen);

            // Return whether it's white's turn to move
            return (board.flags & Constants.flag_white_to_move) != 0UL;
        }

        public void MakeMove(string move)
        {
            if (move.Length == 4 || move.Length == 5)
            {
                char[] x = move.ToCharArray();

                int fromFile = 8 - ((int)x[0] - 96);
                int fromRank = int.Parse(x[1].ToString()) - 1;
                int fromBit = fromRank * 8 + fromFile;

                int toFile = 8 - ((int)x[2] - 96);
                int toRank = int.Parse(x[3].ToString()) - 1;
                int toBit = toRank * 8 + toFile;

                ulong moveflags = 0UL;

                ulong from_mask = 1UL << fromBit;
                ulong to_mask = 1UL << toBit;
                int piece_type = GetPieceTypeOfSquare(board, from_mask);

                //UnitTest test = new UnitTest();
                //test.WriteBits(1UL << fromBit);
                //test.WriteBits(1UL << toBit);

                // Deal with promotions
                if (x.Length == 5)
                {
                    if (x[4] == 'q')
                    {
                        moveflags |= Constants.move_flag_is_promote_queen;
                    }
                    if (x[4] == 'r')
                    {
                        moveflags |= Constants.move_flag_is_promote_rook;
                    }
                    if (x[4] == 'b')
                    {
                        moveflags |= Constants.move_flag_is_promote_bishop;
                    }
                    if (x[4] == 'n')
                    {
                        moveflags |= Constants.move_flag_is_promote_knight;
                    }
                }

                // See if this move is a capture
                if (GetPieceTypeOfSquare(board, to_mask) != -1)
                {
                    moveflags |= Constants.move_flag_is_capture;
                }

                Move m = new Move()
                {
                    mask_from = from_mask,
                    mask_to = to_mask,
                    from_piece_type = piece_type,
                    flags = moveflags
                };

                MakeMove(m);

                ulong test = GetPositionHash(ref board);
            }
            else
            {
                // Invalid move
            }
        }

        public ulong GetPositionHash(ref Board position)
        {
            ulong hash = 0UL;
            ulong square_mask;

            // TODO: Add en-passant hashing

            if ((position.flags & Constants.flag_castle_black_king) != 0UL)
            {
                hash ^= Constants.zobrist_castle_black_king;
            }
            if ((position.flags & Constants.flag_castle_white_king) != 0UL)
            {
                hash ^= Constants.zobrist_castle_white_king;
            }
            if ((position.flags & Constants.flag_castle_black_queen) != 0UL)
            {
                hash ^= Constants.zobrist_castle_black_queen;
            }
            if ((position.flags & Constants.flag_castle_white_queen) != 0UL)
            {
                hash ^= Constants.zobrist_castle_white_queen;
            }

            for (int square = 0; square < 64; square++)
            {
                square_mask = Constants.bit_index_to_mask[square];

                // Is it white to move?
                if ((position.flags & Constants.flag_white_to_move) != 0UL)
                {
                    if ((position.w_king & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[0, square];
                    }
                    else if ((position.w_queen & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[1, square];
                    }
                    else if ((position.w_rook & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[2, square];
                    }
                    else if ((position.w_bishop & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[3, square];
                    }
                    else if ((position.w_knight & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[4, square];
                    }
                    else if ((position.w_pawn & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[5, square];
                    }
                }
                else // Black to move
                {
                    hash ^= Constants.zobrist_black_to_move;

                    if ((position.b_king & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[6, square];
                    }
                    else if ((position.b_queen & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[7, square];
                    }
                    else if ((position.b_rook & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[8, square];
                    }
                    else if ((position.b_bishop & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[9, square];
                    }
                    else if ((position.b_knight & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[10, square];
                    }
                    else if ((position.b_pawn & square_mask) != 0UL)
                    {
                        hash ^= Constants.zobrist_pieces[11, square];
                    }
                }
            }

            return hash;
        }

        public bool IsSquareAttacked(Board position, ulong square_mask, bool origin_is_white_player)
        {
            // It is possible to evaluate a position where the king no longer exists in which case the square mask could be zero.
            // It's a time tradeoff to simply pretend this is possible and keep going.
            if (square_mask == 0UL)
            {
                return true;
            }

            ulong my_pieces;
            ulong enemy_pieces_diag;
            ulong enemy_pieces_cross;
            ulong enemy_pieces_knight;
            ulong enemy_pieces_pawn;
            ulong enemy_pieces_king;
            ulong enemy_all;

            // Are we checking to see if black pieces are attacking one of white's squares?
            if (origin_is_white_player)
            {
                // Get masks of all pieces
                my_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                enemy_pieces_diag = position.b_bishop | position.b_queen;
                enemy_pieces_cross = position.b_rook | position.b_queen;
                enemy_pieces_knight = position.b_knight;
                enemy_pieces_pawn = position.b_pawn;
                enemy_pieces_king = position.b_king;
            }
            else // Black to move
            {
                // Get masks of all pieces
                my_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;
                enemy_pieces_diag = position.w_bishop | position.w_queen ;
                enemy_pieces_cross = position.w_rook | position.w_queen;
                enemy_pieces_knight = position.w_knight;
                enemy_pieces_pawn = position.w_pawn;
                enemy_pieces_king = position.w_king;
            }

            enemy_all = enemy_pieces_diag | enemy_pieces_cross | enemy_pieces_knight | enemy_pieces_pawn | enemy_pieces_king;

            return CanBeCaptured(square_mask, my_pieces, enemy_pieces_diag, enemy_pieces_cross, enemy_pieces_knight, enemy_pieces_pawn, enemy_pieces_king, enemy_all, origin_is_white_player);
        }

        public int GetPieceTypeOfSquare(Board position, ulong square_mask)
        {
            int piece = -1;

            if (((position.w_king | position.b_king) & square_mask) != 0UL)
            {
                return Constants.piece_K;
            }
            if (((position.w_queen | position.b_queen) & square_mask) != 0UL)
            {
                return Constants.piece_Q;
            }
            if (((position.w_rook | position.b_rook) & square_mask) != 0UL)
            {
                return Constants.piece_R;
            }
            if (((position.w_bishop | position.b_bishop) & square_mask) != 0UL)
            {
                return Constants.piece_B;
            }
            if (((position.w_knight | position.b_knight) & square_mask) != 0UL)
            {
                return Constants.piece_N;
            }
            if (((position.w_pawn | position.b_pawn) & square_mask) != 0UL)
            {
                return Constants.piece_P;
            }

            return piece;
        }

        /// <summary>
        /// See if making this move will leave the player in check (invalid move)
        /// </summary>
        public bool TestMove(Move move)
        {
            return TestMove(board, move);
        }

        public bool TestMove(Board position, Move move)
        {
            bool white = (position.flags & Constants.flag_white_to_move) != 0UL;

            MakeMove(ref position, move);

            if (white)
            {
                return !IsSquareAttacked(position, position.w_king, true);
            }
            else
            {
                return !IsSquareAttacked(position, position.b_king, false);
            }
        }

        public void MakeMove(Move move)
        {
            MakeMove(ref board, move);
        }

        private void MakeMove(ref Board position, Move move)
        {
            ulong hash = GetPositionHash(ref position);
            if (positions.ContainsKey(hash))
            {
                positions[hash]++;
            }
            else
            {
                positions.Add(hash, 1);
            }

            // Is it white to move?
            if ((position.flags & Constants.flag_white_to_move) != 0UL)
            {
                // Increment the half move count (number of moves since last "irreversible" move)
                position.half_move_number++;

                // Clear all possible enemy pieces from the target square
                if ((move.flags & Constants.move_flag_is_capture) != 0UL)
                {
                    position.b_king &= ~move.mask_to;
                    position.b_queen &= ~move.mask_to;
                    position.b_rook &= ~move.mask_to;
                    position.b_bishop &= ~move.mask_to;
                    position.b_knight &= ~move.mask_to;
                    position.b_pawn &= ~move.mask_to;
                }

                switch (move.from_piece_type)
                {
                    case Constants.piece_K:
                        // Clear source square
                        position.w_king &= ~move.mask_from;

                        // Fill target square
                        position.w_king |= move.mask_to;

                        // Are we castling? Don't forget to move the rook! We'll just assume it's there since the king moved two squares...
                        if (move.mask_from == Constants.mask_E1)
                        {
                            if (move.mask_to == Constants.mask_G1)
                            {
                                // Kingside castle
                                position.w_rook &= ~Constants.mask_H1;
                                position.w_rook |= Constants.mask_F1;
                            }
                            else if (move.mask_to == Constants.mask_C1)
                            {
                                // Queenside castle
                                position.w_rook &= ~Constants.mask_A1;
                                position.w_rook |= Constants.mask_D1;
                            }
                        }

                        // The king moved, so the castling privelage is now gone
                        position.flags &= ~Constants.flag_castle_white_queen;
                        position.flags &= ~Constants.flag_castle_white_king;

                        break;
                    case Constants.piece_Q:
                        position.w_queen &= ~move.mask_from;
                        position.w_queen |= move.mask_to;
                        break;
                    case Constants.piece_R:
                        position.w_rook &= ~move.mask_from;
                        position.w_rook |= move.mask_to;

                        // Clear the corresponding castling flag for the side that this rook came from
                        if (move.mask_from == Constants.mask_A1)
                        {
                            position.flags &= ~Constants.flag_castle_white_queen;
                        }
                        if (move.mask_from == Constants.mask_H1)
                        {
                            position.flags &= ~Constants.flag_castle_white_king;
                        }
                        break;
                    case Constants.piece_B:
                        position.w_bishop &= ~move.mask_from;
                        position.w_bishop |= move.mask_to;
                        break;
                    case Constants.piece_N:
                        position.w_knight &= ~move.mask_from;
                        position.w_knight |= move.mask_to;
                        break;
                    case Constants.piece_P:
                        position.w_pawn &= ~move.mask_from;

                        // Is this a 2 square jump? Set the en-passent square 
                        if (move.mask_to == (move.mask_from << 16))
                        {
                            position.en_passent_square = move.mask_from << 8;
                        }
                        else
                        {
                            position.en_passent_square = 0UL;
                        }

                        // Is this an en-passant capture?
                        if ((move.flags & Constants.move_flag_is_en_passent) != 0UL)
                        {
                            // Remove the pawn that jumped past your destination square
                            position.b_pawn &= ~(move.mask_to >> 8);
                        }

                        // Deal with promotions
                        if ((move.flags & Constants.move_flag_is_promote_bishop) != 0UL)
                        {
                            position.w_bishop |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_knight) != 0UL)
                        {
                            position.w_knight |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_rook) != 0UL)
                        {
                            position.w_rook |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_queen) != 0UL)
                        {
                            position.w_queen |= move.mask_to;
                        }
                        else
                        {
                            position.w_pawn |= move.mask_to;
                        }
                        break;
                }

                // It's now black's turn
                position.flags &= ~Constants.flag_white_to_move;
            }
            else
            {
                // After each move by black we increment the full move counter
                position.move_number++;
                position.half_move_number++;

                // Clear all possible enemy pieces from the target square
                if ((move.flags & Constants.move_flag_is_capture) != 0UL)
                {
                    position.w_king &= ~move.mask_to;
                    position.w_queen &= ~move.mask_to;
                    position.w_rook &= ~move.mask_to;
                    position.w_bishop &= ~move.mask_to;
                    position.w_knight &= ~move.mask_to;
                    position.w_pawn &= ~move.mask_to;
                }

                switch (move.from_piece_type)
                {
                    case Constants.piece_K:
                        position.b_king &= ~move.mask_from;
                        position.b_king |= move.mask_to;

                        if (move.mask_from == Constants.mask_E8)
                        {
                            if (move.mask_to == Constants.mask_G8)
                            {
                                // Kingside castle
                                position.b_rook &= ~Constants.mask_H8;
                                position.b_rook |= Constants.mask_F8;
                            }
                            else if (move.mask_to == Constants.mask_C8)
                            {
                                // Queenside castle
                                position.b_rook &= ~Constants.mask_A8;
                                position.b_rook |= Constants.mask_D8;
                            }
                        }

                        position.flags &= ~Constants.flag_castle_black_king;
                        position.flags &= ~Constants.flag_castle_black_queen;
                        break;
                    case Constants.piece_Q:
                        position.b_queen &= ~move.mask_from;
                        position.b_queen |= move.mask_to;
                        break;
                    case Constants.piece_R:
                        position.b_rook &= ~move.mask_from;
                        position.b_rook |= move.mask_to;

                        // Clear the corresponding castling flag for the side that this rook came from
                        if (move.mask_from == Constants.mask_A8)
                        {
                            position.flags &= ~Constants.flag_castle_black_queen;
                        }
                        if (move.mask_from == Constants.mask_H8)
                        {
                            position.flags &= ~Constants.flag_castle_black_king;
                        }
                        break;
                    case Constants.piece_B:
                        position.b_bishop &= ~move.mask_from;
                        position.b_bishop |= move.mask_to;
                        break;
                    case Constants.piece_N:
                        position.b_knight &= ~move.mask_from;
                        position.b_knight |= move.mask_to;
                        break;
                    case Constants.piece_P:
                        position.b_pawn &= ~move.mask_from;

                        if (move.mask_to == (move.mask_from >> 16))
                        {
                            position.en_passent_square = move.mask_from >> 8;
                        }
                        else
                        {
                            position.en_passent_square = 0UL;
                        }

                        if ((move.flags & Constants.move_flag_is_en_passent) != 0UL)
                        {
                            // Remove the pawn that jumped past your destination square
                            position.w_pawn &= ~(move.mask_to << 8);
                        }

                        // Deal with promotions
                        if ((move.flags & Constants.move_flag_is_promote_bishop) != 0UL)
                        {
                            position.b_bishop |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_knight) != 0UL)
                        {
                            position.b_knight |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_rook) != 0UL)
                        {
                            position.b_rook |= move.mask_to;
                        }
                        else if ((move.flags & Constants.move_flag_is_promote_queen) != 0UL)
                        {
                            position.b_queen |= move.mask_to;
                        }
                        else
                        {
                            position.b_pawn |= move.mask_to;
                        }
                        break;
                }

                // It's now white's turn
                position.flags |= Constants.flag_white_to_move;
            }

            // Reset the half move counter on any capture or pawn move
            if (move.from_piece_type == Constants.piece_P || (position.flags & Constants.move_flag_is_capture) != 0UL)
            {
                position.half_move_number = 0;
            }
        }

        public Move GetBestMove(int seconds = -1, int search_depth = -1)
        {
            stopwatch = Stopwatch.StartNew();
            evals = 0;
            cutoff = false;

            time_per_move = seconds == -1 ? 10 : seconds;
            depth_per_move = search_depth == -1 ? 99 : search_depth;

            Move best = new Move();
            Move search;

            Timer timer = new Timer(500);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            List<Move> all_moves = new List<Move>();
            int moves_count = 0;

            // Iterative deepening
            for (int depth = 2; depth <= depth_per_move; depth += 2)
            {
                search = GetBestMove(ref board, depth, Int32.MinValue, Int32.MaxValue, depth /* TODO: Selective search */, all_moves, ref moves_count, true, true);

                if (!cutoff)
                {
                    best = search;
                }

                // Order the moves based on the results of the previous iteration (slow, but we only do it once per depth change)
                List<Move> moves = all_moves.Where(x => x.mask_to != 0UL).ToList();
                if (board.WhiteToPlay())
                {
                    moves.Sort((c, n) => n.evaluation.CompareTo(c.evaluation));
                }
                else
                {
                    moves.Sort((c, n) => c.evaluation.CompareTo(n.evaluation));
                }
                for (int i = 0; i < Math.Min(moves.Count, moves_count); i++) // TODO: Math.Min(moves_count, moves_count) should be unnecessary... They should be identical
                {
                    all_moves[i] = moves[i];
                }
            }

            timer.Stop();

            return best;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > time_per_move * 1000)
            {
                cutoff = true;
            }

            long nps = (long)((double)evals / ((double)stopwatch.ElapsedMilliseconds / 1000.0));
            Console.WriteLine("info nps " + nps +
                            " nodes " + evals);
        }

        private Move GetBestMove(ref Board position, int depth, int alpha, int beta, int selective, List<Move> all_moves, ref int all_moves_count, bool all_moves_update, bool firstlevel = false)
        {
            // Stop calculating and toss any results
            if (cutoff)
            {
                return new Move();
            }

            int startevals = evals;

            // See if we can look up a previous calculation first
            ulong hash1 = GetPositionHash(ref position);
            ulong key1 = hash1 & evaluations_max_mask;
            Move m = evaluations[key1];
            if (m.depth >= depth && m.selective == (depth < 0))
            {
                if (evaluations_positions[key1].Equals(position))
                {
                    // evals += m.evals;
                    return m;
                }
            }

            // Reached max depth of search
            if (depth <= 0 && selective <= 0)
            {
                return new Move()
                {
                    evaluation = EvaluateBoard(ref position)
                };
            }

            bool white_to_play = (position.flags & Constants.flag_white_to_move) != 0UL;

            int count;
            List<Move> moves;
            if (all_moves_count > 0)
            {
                moves = all_moves;
                count = all_moves_count;
            }
            else
            {
                moves = GetAllMoves(position, out count, depth < 0);
            }
            Move bestmove = new Move();
            bestmove.evaluation = white_to_play ? int.MinValue : int.MaxValue;
            Move testmove;
            Board temp;
            bool set = false;
            ulong hash;
            bool improved = false;
            int zero = 0;

            for (int move_num = 0; move_num < moves.Count; move_num++)
            {
                // Limit to captures for selective searches
                if ((moves[move_num].flags & Constants.move_flag_is_capture) == 0UL && depth <= 0)
                {
                    continue;
                }

                // Copy game state
                temp = position;

                // Make the suggested move
                MakeMove(ref temp, moves[move_num]);

                // Detect 3 fold repetition
                hash = GetPositionHash(ref position);

                // TODO: This won't work for multi-threaded tree searches...
                if (positions[hash] >= 3)
                {
                    // It's a dead draw
                    testmove.evaluation = 0;
                }

                // Is the move valid?
                if (white_to_play)
                {
                    if (IsSquareAttacked(temp, temp.w_king, true))
                    {
                        continue;
                    }
                }
                else
                {
                    if (IsSquareAttacked(temp, temp.b_king, false))
                    {
                        continue;
                    }
                }

                // Evaluate the counter moves
                testmove = GetBestMove(ref temp, depth - 1, alpha, beta, selective - 1, null, ref zero, false);

                // Remove the evaluated move from the hash table
                positions[hash]--;
                if (positions[hash] <= 0)
                {
                    // Clean up some of the millions of empty records...
                    positions.Remove(hash);
                }

                // Save the evaluation for move ordering
                if (all_moves_update)
                {
                    all_moves_count = count;
                    Move m2 = moves[move_num];
                    m2.evaluation = testmove.evaluation;
                    if (all_moves.Count <= move_num)
                    {
                        all_moves.Add(m2);
                    }
                    else
                    {
                        all_moves[move_num] = m2;
                    }
                }

                // Compute fastest mate by reducing score through levels
                if (testmove.evaluation > Constants.eval_king_loss_threshold)
                {
                    testmove.evaluation -= Constants.eval_adjust_king_loss;
                }
                if (testmove.evaluation < -Constants.eval_king_loss_threshold)
                {
                    testmove.evaluation += Constants.eval_adjust_king_loss;
                }

                if (white_to_play)
                {
                    if (testmove.evaluation > bestmove.evaluation || !set)
                    {
                        bestmove = moves[move_num];
                        bestmove.evaluation = testmove.evaluation;
                        bestmove.primary_variation = moves[move_num].ToString() + " " + testmove.primary_variation;
                        set = true;
                        improved = true;

                        alpha = testmove.evaluation;
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (testmove.evaluation < bestmove.evaluation || !set)
                    {
                        bestmove = moves[move_num];
                        bestmove.evaluation = testmove.evaluation;
                        bestmove.primary_variation = moves[move_num].ToString() + " " + testmove.primary_variation;
                        set = true;
                        improved = true;

                        beta = testmove.evaluation;
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }

                // Display some stats if we are at the base level of recursion
                if (firstlevel && !cutoff && improved)
                {
                    if (improved)
                    {
                        Console.WriteLine("info score cp " + bestmove.evaluation +
                            " depth " + (depth / 2) +
                            " nodes " + evals +
                            " time " + stopwatch.ElapsedMilliseconds +
                            " currmove " + moves[move_num] +
                            " pv " + bestmove.primary_variation);
                        improved = false;
                    }
                }
            }

            //if (!evaluations.ContainsKey(hash1))
            //{
            //    bestmove.depth = depth;
            //    bestmove.evals = evals - startevals;
            //    evaluations.Add(hash1, bestmove);
            //}

            // Save the evaluation
            bestmove.depth = depth;
            bestmove.evals = evals - startevals;
            bestmove.selective = depth < 0;
            evaluations[key1] = bestmove;
            evaluations_positions[key1] = position;

            if (depth < 0)
            {
                // We are in a selective search but didn't have any moves to evaluate
                if (bestmove.evaluation == (white_to_play ? int.MinValue : int.MaxValue))
                {
                    return new Move()
                    {
                        evaluation = EvaluateBoard(ref position)
                    };
                }
            }

            return bestmove;
        }

        private int EvaluateBoard(ref Board position)
        {
            //// See if we can look up a previous calculation first
            //ulong hash = GetPositionHash(ref position);
            //if (evaluations.ContainsKey(hash))
            //{
            //    return evaluations[hash];
            //}

            // Start with a random evaluation to mix it up ever so slightly when there's two equal moves
            int eval = Utility.Rand.Next(3) - 1;
            evals++;

            ulong square_mask;

            EvaluateMaterial(ref position, ref eval);

            EvaluateDevelopment(ref position, ref eval);

            // Evaluate piece placement
            for (int square = 0; square < 64; square++)
            {
                square_mask = Constants.bit_index_to_mask[square];

                // White
                if ((position.w_king & square_mask) != 0UL)
                {
                    // TODO: consider the end game where it's better to be in the middle
                    eval += Constants.piece_square_value_white_king_middle[square];
                }
                else if ((position.w_queen & square_mask) != 0UL)
                {
                    eval += Constants.piece_square_value_white_queen[square];
                }
                else if ((position.w_rook & square_mask) != 0UL)
                {
                    eval += Constants.piece_square_value_white_rook[square];
                }
                else if ((position.w_bishop & square_mask) != 0UL)
                {
                    eval += Constants.piece_square_value_white_bishop[square];
                }
                else if ((position.w_knight & square_mask) != 0UL)
                {
                    eval += Constants.piece_square_value_white_knight[square];
                }
                else if ((position.w_pawn & square_mask) != 0UL)
                {
                    eval += Constants.piece_square_value_white_pawn[square];
                }

                // Black
                if ((position.b_king & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_king_middle[square];
                }
                else if ((position.b_queen & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_queen[square];
                }
                else if ((position.b_rook & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_rook[square];
                }
                else if ((position.b_bishop & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_bishop[square];
                }
                else if ((position.b_knight & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_knight[square];
                }
                else if ((position.b_pawn & square_mask) != 0UL)
                {
                    eval -= Constants.piece_square_value_black_pawn[square];
                }
            }

            // Evaluate pawn structure
            EvaluatePawnStructure(ref position, ref eval);

            // Evaluate piece mobility
            // TODO

            //evaluations.Add(hash, eval);

            return eval;
        }

        private static void EvaluatePawnStructure(ref Board position, ref int eval)
        {

        }

        private static void EvaluateDevelopment(ref Board position, ref int eval)
        {
            eval -= (position.w_bishop & Constants.mask_F1) != 0UL ? Constants.eval_develop_piece : 0;
            eval -= (position.w_bishop & Constants.mask_C1) != 0UL ? Constants.eval_develop_piece : 0;
            eval -= (position.w_knight & Constants.mask_G1) != 0UL ? Constants.eval_develop_piece : 0;
            eval -= (position.w_knight & Constants.mask_B1) != 0UL ? Constants.eval_develop_piece : 0;

            eval += (position.b_bishop & Constants.mask_F8) != 0UL ? Constants.eval_develop_piece : 0;
            eval += (position.b_bishop & Constants.mask_C8) != 0UL ? Constants.eval_develop_piece : 0;
            eval += (position.b_knight & Constants.mask_G8) != 0UL ? Constants.eval_develop_piece : 0;
            eval += (position.b_knight & Constants.mask_B8) != 0UL ? Constants.eval_develop_piece : 0;
        }

        //private static void EvaluateKingSafety(ref Board position, ref int eval)
        //{
        //    eval += (position.w_king & Constants.king_safety_level_0) != 0UL ? Constants.king_safety_level_0_centipawns : 0;
        //    eval += (position.w_king & Constants.king_safety_level_1) != 0UL ? Constants.king_safety_level_1_centipawns : 0;
        //    eval += (position.w_king & Constants.king_safety_level_2) != 0UL ? Constants.king_safety_level_2_centipawns : 0;
        //    eval += (position.w_king & Constants.king_safety_best_ranks) != 0UL ? Constants.king_safety_best_ranks_centipawns : 0;

        //    eval -= (position.b_king & Constants.king_safety_level_0) != 0UL ? Constants.king_safety_level_0_centipawns : 0;
        //    eval -= (position.b_king & Constants.king_safety_level_1) != 0UL ? Constants.king_safety_level_1_centipawns : 0;
        //    eval -= (position.b_king & Constants.king_safety_level_2) != 0UL ? Constants.king_safety_level_2_centipawns : 0;
        //    eval -= (position.b_king & Constants.king_safety_best_ranks) != 0UL ? Constants.king_safety_best_ranks_centipawns : 0;
        //}

        private void EvaluateMaterial(ref Board position, ref int eval)
        {
            int diff = 0;

            //// See if we can look up a previous calculation first
            //ulong hash = GetPositionHash(ref position);
            //ulong key = hash & material_max_mask;
            //int saved_eval = material_eval[key];

            //if (material_positions[key].Equals(position))
            //{
            //    diff = saved_eval;
            //}
            //else
            //{
                diff += Utility.CountBits(position.w_king) * Constants.eval_king;
                diff += Utility.CountBits(position.w_queen) * Constants.eval_queen;
                diff += Utility.CountBits(position.w_rook) * Constants.eval_rook;
                diff += Utility.CountBits(position.w_bishop) * Constants.eval_bishop;
                diff += Utility.CountBits(position.w_knight) * Constants.eval_knight;
                diff += Utility.CountBits(position.w_pawn) * Constants.eval_pawn;

                diff -= Utility.CountBits(position.b_king) * Constants.eval_king;
                diff -= Utility.CountBits(position.b_queen) * Constants.eval_queen;
                diff -= Utility.CountBits(position.b_rook) * Constants.eval_rook;
                diff -= Utility.CountBits(position.b_bishop) * Constants.eval_bishop;
                diff -= Utility.CountBits(position.b_knight) * Constants.eval_knight;
                diff -= Utility.CountBits(position.b_pawn) * Constants.eval_pawn;

            //    // Save the calculation
            //    material_eval[key] = diff;
            //    material_positions[key] = position;
            //}

            eval += diff;
        }

        public Move GetRandomMove()
        {
            int count;
            List<Move> moves = GetAllMoves(board, out count);
            Random r = new Random();

            while (moves.Count > 0)
            {
                int i = r.Next(moves.Count);
                Move m = moves[i];
                if (TestMove(m))
                {
                    return m;
                }
                else
                {
                    moves.RemoveAt(i);
                }
            }

            // Checkmate
            return new Move();
        }

        public List<Move> GetAllMoves(Board position, out int moves_count, bool capturesOnly = false)
        {
            List<Move> moves = new List<Move>();

            ulong square_mask;
            ulong my_pieces;
            ulong enemy_pieces;

            // Is it white to move?
            if ((position.flags & Constants.flag_white_to_move) != 0UL)
            {
                // Get masks of all pieces
                my_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                enemy_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;
            }
            else
            {
                enemy_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                my_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;
            }

            for (int square = 0; square < 64; square++)
            {
                square_mask = Constants.bit_index_to_mask[square];

                // Is it white to move?
                if ((position.flags & Constants.flag_white_to_move) != 0UL)
                {
                    // King
                    if ((position.w_king & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_K, my_pieces, enemy_pieces, true, capturesOnly);
                    }

                    // Queen
                    else if ((position.w_queen & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_Q, my_pieces, enemy_pieces, true, capturesOnly);
                    }

                    // Rook
                    else if ((position.w_rook & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_R, my_pieces, enemy_pieces, true, capturesOnly);
                    }

                    // Bishop
                    else if ((position.w_bishop & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_B, my_pieces, enemy_pieces, true, capturesOnly);
                    }

                    // Knight
                    else if ((position.w_knight & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_N, my_pieces, enemy_pieces, true, capturesOnly);
                    }

                    // Pawn
                    else if ((position.w_pawn & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_P, my_pieces, enemy_pieces, true, capturesOnly);
                    }
                }
                else // Black to move
                {
                    // King
                    if ((position.b_king & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_K, my_pieces, enemy_pieces, false, capturesOnly);
                    }

                    // Queen
                    else if ((position.b_queen & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_Q, my_pieces, enemy_pieces, false, capturesOnly);
                    }

                    // Rook
                    else if ((position.b_rook & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_R, my_pieces, enemy_pieces, false, capturesOnly);
                    }

                    // Bishop
                    else if ((position.b_bishop & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_B, my_pieces, enemy_pieces, false, capturesOnly);
                    }

                    // Knight
                    else if ((position.b_knight & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_N, my_pieces, enemy_pieces, false, capturesOnly);
                    }

                    // Pawn
                    else if ((position.b_pawn & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref position, square, square_mask, Constants.piece_P, my_pieces, enemy_pieces, false, capturesOnly);
                    }
                }
            }

            moves_count = moves.Count;
            return moves;
        }

        private void GetMovesForPiece(ref List<Move> moves, ref Board position, int square, ulong square_mask, int pieceType, ulong my_pieces, ulong enemy_pieces, bool white_to_play, bool capturesOnly)
        {
            ulong destination;
            ulong moveflags;
            ulong clearsquares;
            bool capture;
            bool promotion = false;

            // Take care of castling moves for the king
            // The king cannot castle through check (although the rook may)
            if (pieceType == Constants.piece_K)
            {
                if (white_to_play)
                {
                    if ((position.flags & Constants.flag_castle_white_king) != 0UL)
                    {
                        // Short castle
                        clearsquares = Constants.mask_F1 | Constants.mask_G1;
                        if (position.w_king == Constants.mask_E1 &&
                            (position.w_rook & Constants.mask_H1) != 0UL &&
                            !IsSquareAttacked(position, Constants.mask_E1, true) &&
                            !IsSquareAttacked(position, Constants.mask_F1, true) &&
                            !IsSquareAttacked(position, Constants.mask_G1, true) &&
                            (my_pieces & clearsquares) == 0UL &&
                            (enemy_pieces & clearsquares) == 0UL)
                        {
                            moves.Add(new Move()
                            {
                                mask_from = square_mask,
                                mask_to = Constants.mask_G1,
                                flags = Constants.move_flag_is_castle_short,
                                from_piece_type = pieceType
                            });
                        }
                    }

                    if ((position.flags & Constants.flag_castle_white_queen) != 0UL)
                    {
                        // Long castle
                        clearsquares = Constants.mask_D1 | Constants.mask_C1 | Constants.mask_B1;
                        if (position.w_king == Constants.mask_E1 &&
                            (position.w_rook & Constants.mask_A1) != 0UL &&
                            !IsSquareAttacked(position, Constants.mask_E1, true) &&
                            !IsSquareAttacked(position, Constants.mask_D1, true) &&
                            !IsSquareAttacked(position, Constants.mask_C1, true) &&
                            (my_pieces & clearsquares) == 0UL &&
                            (enemy_pieces & clearsquares) == 0UL)
                        {
                            moves.Add(new Move()
                            {
                                mask_from = square_mask,
                                mask_to = Constants.mask_C1,
                                flags = Constants.move_flag_is_castle_long,
                                from_piece_type = pieceType
                            });
                        }
                    }
                }
                else
                {
                    if ((position.flags & Constants.flag_castle_black_king) != 0UL)
                    {
                        // Short castle
                        clearsquares = Constants.mask_F8 | Constants.mask_G8;
                        if (position.b_king == Constants.mask_E8 &&
                            (position.b_rook & Constants.mask_H8) != 0UL &&
                            !IsSquareAttacked(position, Constants.mask_E8, false) &&
                            !IsSquareAttacked(position, Constants.mask_F8, false) &&
                            !IsSquareAttacked(position, Constants.mask_G8, false) &&
                            (my_pieces & clearsquares) == 0UL &&
                            (enemy_pieces & clearsquares) == 0UL)
                        {
                            moves.Add(new Move()
                            {
                                mask_from = square_mask,
                                mask_to = Constants.mask_G8,
                                flags = Constants.move_flag_is_castle_short,
                                from_piece_type = pieceType
                            });
                        }
                    }

                    if ((position.flags & Constants.flag_castle_black_queen) != 0UL)
                    {
                        // Long castle
                        clearsquares = Constants.mask_D8 | Constants.mask_C8 | Constants.mask_B8;
                        if (position.b_king == Constants.mask_E8 &&
                            (position.b_rook & Constants.mask_A8) != 0UL &&
                            !IsSquareAttacked(position, Constants.mask_E8, false) &&
                            !IsSquareAttacked(position, Constants.mask_D8, false) &&
                            !IsSquareAttacked(position, Constants.mask_C8, false) &&
                            (my_pieces & clearsquares) == 0UL &&
                            (enemy_pieces & clearsquares) == 0UL)
                        {
                            moves.Add(new Move()
                            {
                                mask_from = square_mask,
                                mask_to = Constants.mask_C8,
                                flags = Constants.move_flag_is_castle_long,
                                from_piece_type = pieceType
                            });
                        }
                    }
                }
            }

            // Loop through directions
            for (int d = 0; d < 8; d++)
            {
                // Loop through movements withing this direction in order 
                for (int m = 0; m < 8; m++)
                {
                    destination = Constants.movements[square, pieceType, d, m];

                    // End of move chain?
                    if (destination == Constants.NULL)
                    {
                        break;
                    }

                    // Do we already have one of our own pieces on this square?
                    if ((destination & my_pieces) != 0UL)
                    {
                        break;
                    }

                    moveflags = 0UL;
                    capture = false;

                    // Is this move a capture?
                    if ((destination & enemy_pieces) != 0UL)
                    {
                        moveflags |= Constants.move_flag_is_capture;
                        capture = true;
                    }
                    else if (capturesOnly)
                    {
                        continue;
                    }

                    // Take care of all the joys of pawn calculation...
                    if (pieceType == Constants.piece_P)
                    {
                        if (white_to_play)
                        {
                            // Make sure the pawns don't try to move backwards
                            if (destination < square_mask)
                            {
                                break;
                            }

                            // En-passant - run BEFORE general capture checking since this won't normally be considered a capture (no piece on target square)
                            if (destination == position.en_passent_square && position.en_passent_square != 0UL)
                            {
                                moveflags |= Constants.move_flag_is_en_passent;
                                moveflags |= Constants.move_flag_is_capture;
                            }
                            else
                            {
                                // Make sure the pawns only move sideways if they are capturing
                                if (destination == (square_mask << 9) && !capture)
                                {
                                    break;
                                }
                                if (destination == (square_mask << 7) && !capture)
                                {
                                    break;
                                }
                            }

                            // Make sure pawns don't try to capture on an initial 2 square jump or general forward move
                            if (destination == (square_mask << 16) && capture)
                            {
                                break;
                            }
                            if (destination == (square_mask << 8) && capture)
                            {
                                break;
                            }

                            // Deal with promotions
                            if ((destination & 0xFF00000000000000UL) != 0UL)
                            {
                                promotion = true;
                            }
                        }
                        else
                        {
                            // Make sure the pawns don't try to move backwards
                            if (destination > square_mask)
                            {
                                break;
                            }

                            // En-passant - run BEFORE general capture checking since this won't normally be considered a capture (no piece on target square)
                            if (destination == position.en_passent_square && position.en_passent_square != 0UL)
                            {
                                moveflags |= Constants.move_flag_is_en_passent;
                                moveflags |= Constants.move_flag_is_capture;
                            }
                            else
                            {
                                // Make sure the pawns only move sideways if they are capturing
                                if (destination == (square_mask >> 9) && !capture)
                                {
                                    break;
                                }
                                if (destination == (square_mask >> 7) && !capture)
                                {
                                    break;
                                }
                            }

                            // Make sure pawns don't try to capture on an initial 2 square jump or general forward move
                            if (destination == (square_mask >> 16) && capture)
                            {
                                break;
                            }
                            if (destination == (square_mask >> 8) && capture)
                            {
                                break;
                            }

                            // Deal with promotions
                            if ((destination & 0x00000000000000FFUL) != 0UL)
                            {
                                promotion = true;
                            }
                        }
                    }

                    if (promotion)
                    {
                        // Enter all 4 possible promotion types
                        moves.Add(new Move()
                        {
                            mask_from = square_mask,
                            mask_to = destination,
                            flags = moveflags | Constants.move_flag_is_promote_bishop,
                            from_piece_type = pieceType
                        });
                        moves.Add(new Move()
                        {
                            mask_from = square_mask,
                            mask_to = destination,
                            flags = moveflags | Constants.move_flag_is_promote_knight,
                            from_piece_type = pieceType
                        });
                        moves.Add(new Move()
                        {
                            mask_from = square_mask,
                            mask_to = destination,
                            flags = moveflags | Constants.move_flag_is_promote_rook,
                            from_piece_type = pieceType
                        });
                        moves.Add(new Move()
                        {
                            mask_from = square_mask,
                            mask_to = destination,
                            flags = moveflags | Constants.move_flag_is_promote_queen,
                            from_piece_type = pieceType
                        });
                    }
                    else
                    {
                        // Enter a regular move 
                        moves.Add(new Move()
                        {
                            mask_from = square_mask,
                            mask_to = destination,
                            flags = moveflags,
                            from_piece_type = pieceType
                        });
                    }

                    // We found a capture searching down this direction, so stop looking further
                    if (capture)
                    {
                        break;
                    }
                }
            }
        }

        private bool CanBeCaptured(ulong square_mask, ulong my_pieces, ulong enemy_diag, ulong enemy_cross, ulong enemy_knight, ulong enemy_pawn, ulong enemy_king, ulong enemy_all, bool source_is_white_piece)
        {
            ulong destination;
            int from_square = Utility.GetIndexFromMask(square_mask);

            // Check for king captures
            for (int d = 0; d < 8; d++)
            {
                destination = Constants.movements[from_square, Constants.piece_K, d, 0];

                // End of move chain?
                if (destination == Constants.NULL)
                {
                    break;
                }

                // Do we already have one of our own pieces on this square?
                if ((destination & my_pieces) != 0UL)
                {
                    break;
                }

                // Is this move a capture?
                if ((destination & enemy_king) != 0UL)
                {
                    return true;
                }

                // Is this hitting any other random enemy piece that can't attack us?
                if ((destination & enemy_all) != 0UL)
                {
                    break;
                }
            }

            // Check for diagonal captures
            for (int d = 0; d < 4; d++)
            {
                // Loop through movements within this direction in order 
                for (int m = 0; m < 8; m++)
                {
                    destination = Constants.movements[from_square, Constants.piece_B, d, m];

                    // End of move chain?
                    if (destination == Constants.NULL)
                    {
                        break;
                    }

                    // Do we already have one of our own pieces on this square?
                    if ((destination & my_pieces) != 0UL)
                    {
                        break;
                    }

                    // Is this move a capture?
                    if ((destination & enemy_diag) != 0UL)
                    {
                        return true;
                    }

                    // Is this hitting any other random enemy piece that can't attack us?
                    if ((destination & enemy_all) != 0UL)
                    {
                        break;
                    }
                }
            }

            // Check for horizontal and vertical captures
            for (int d = 0; d < 4; d++)
            {
                // Loop through movements within this direction in order 
                for (int m = 0; m < 8; m++)
                {
                    destination = Constants.movements[from_square, Constants.piece_R, d, m];

                    // End of move chain?
                    if (destination == Constants.NULL)
                    {
                        break;
                    }

                    // Do we already have one of our own pieces on this square?
                    if ((destination & my_pieces) != 0UL)
                    {
                        break;
                    }

                    // Is this move a capture?
                    if ((destination & enemy_cross) != 0UL)
                    {
                        return true;
                    }

                    // Is this hitting any other random enemy piece that can't attack us?
                    if ((destination & enemy_all) != 0UL)
                    {
                        break;
                    }
                }
            }

            // Check for knight captures
            for (int d = 0; d < 8; d++)
            {
                // Loop through movements within this direction in order 
                for (int m = 0; m < 8; m++)
                {
                    destination = Constants.movements[from_square, Constants.piece_N, d, m];

                    // End of move chain?
                    if (destination == Constants.NULL)
                    {
                        break;
                    }

                    // Do we already have one of our own pieces on this square?
                    if ((destination & my_pieces) != 0UL)
                    {
                        break;
                    }

                    // Is this move a capture?
                    if ((destination & enemy_knight) != 0UL)
                    {
                        return true;
                    }
                }
            }

            // Check for pawn captures
            for (int d = 0; d < 8; d++)
            {
                // Loop through movements within this direction in order 
                for (int m = 0; m < 8; m++)
                {
                    destination = Constants.movements[from_square, Constants.piece_P, d, m];

                    // End of move chain?
                    if (destination == Constants.NULL)
                    {
                        break;
                    }

                    // Do we already have one of our own pieces on this square?
                    if ((destination & my_pieces) != 0UL)
                    {
                        break;
                    }

                    // Is this move a capture?
                    if ((destination & enemy_pawn) != 0UL)
                    {
                        if (source_is_white_piece)
                        {
                            if (destination == (square_mask << 9))
                            {
                                return true;
                            }
                            if (destination == (square_mask << 7))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (destination == (square_mask >> 9))
                            {
                                return true;
                            }
                            if (destination == (square_mask >> 7))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}

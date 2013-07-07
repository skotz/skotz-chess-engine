using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class Game
    {
        public Board board;

        public Game()
        {
            board = new Board();
        }

        public void ResetBoard()
        {
            board = BoardGenerator.NewStandardSetup();
        }

        public void MakeMove(string move)
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
        }

        public bool IsSquareAttacked(Board position, ulong square_mask, bool origin_is_white_player)
        {
            ulong my_pieces;
            ulong enemy_pieces_diag;
            ulong enemy_pieces_cross;
            ulong enemy_pieces_knight;
            ulong enemy_pieces_pawn;
            ulong enemy_all;

            // Are we checking to see if black pieces are attacking one of white's squares?
            if (origin_is_white_player)
            {
                // Get masks of all pieces
                my_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                enemy_pieces_diag = position.b_bishop | position.b_queen | position.b_king;
                enemy_pieces_cross = position.b_rook | position.b_queen | position.b_king;
                enemy_pieces_knight = position.b_knight;
                enemy_pieces_pawn = position.b_pawn;
            }
            else // Black to move
            {
                // Get masks of all pieces
                my_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;
                enemy_pieces_diag = position.w_bishop | position.w_queen | position.w_king;
                enemy_pieces_cross = position.w_rook | position.w_queen | position.w_king;
                enemy_pieces_knight = position.w_knight;
                enemy_pieces_pawn = position.w_pawn;
            }

            enemy_all = enemy_pieces_diag | enemy_pieces_cross | enemy_pieces_knight | enemy_pieces_pawn;

            return CanBeCaptured(square_mask, my_pieces, enemy_pieces_diag, enemy_pieces_cross, enemy_pieces_knight, enemy_pieces_pawn, enemy_all, origin_is_white_player);
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
            Board copy = board;
            bool white = (copy.flags & Constants.flag_white_to_move) != 0UL;

            MakeMove(ref copy, move);

            if (white)
            {
                return !IsSquareAttacked(copy, copy.w_king, true);
            }
            else
            {
                return !IsSquareAttacked(copy, copy.b_king, false);
            }
        }

        public void MakeMove(Move move)
        {
            MakeMove(ref board, move);
        }

        private void MakeMove(ref Board position, Move move)
        {
            // Is it white to move?
            if ((position.flags & Constants.flag_white_to_move) != 0UL)
            {
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
                        if (move.mask_from == (move.mask_to >> 2))
                        {
                            // Kingside
                            position.w_rook &= ~Constants.mask_H1;
                            position.w_rook |= Constants.mask_F1;
                        }
                        if (move.mask_from == (move.mask_to << 2))
                        {
                            // Queenside 
                            position.w_rook &= ~Constants.mask_A1;
                            position.w_rook |= Constants.mask_D1;
                        }
                        break;
                    case Constants.piece_Q:
                        position.w_queen &= ~move.mask_from;
                        position.w_queen |= move.mask_to;
                        break;
                    case Constants.piece_R:
                        position.w_rook &= ~move.mask_from;
                        position.w_rook |= move.mask_to;
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

                        if (move.mask_from == (move.mask_to >> 2))
                        {
                            // Kingside castle
                            position.w_rook &= ~Constants.mask_H8;
                            position.w_rook |= Constants.mask_F8;
                        }
                        if (move.mask_from == (move.mask_to << 2))
                        {
                            // Queenside castle
                            position.w_rook &= ~Constants.mask_A8;
                            position.w_rook |= Constants.mask_D8;
                        }
                        break;
                    case Constants.piece_Q:
                        position.b_queen &= ~move.mask_from;
                        position.b_queen |= move.mask_to;
                        break;
                    case Constants.piece_R:
                        position.b_rook &= ~move.mask_from;
                        position.b_rook |= move.mask_to;
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
        }

        public Move GetBestMove()
        {
            // TODO... Obviously
            return GetRandomMove();
        }

        public Move GetRandomMove()
        {
            int count;
            Move[] moves = GetAllMoves(board, out count);
            List<Move> all = moves.ToList().Where(x => x.mask_from != 0).ToList();
            Random r = new Random();

            while (all.Count > 0)
            {
                int i = r.Next(all.Count);
                Move m = all[i];
                if (TestMove(m))
                {
                    return m;
                }
                else
                {
                    all.RemoveAt(i);
                }
            }

            // Checkmate
            return new Move();
        }

        public Move[] GetAllMoves(Board position, out int moves_count)
        {
            Move[] moves = new Move[230];
            int index = 0;

            ulong square_mask;
            ulong my_pieces;
            ulong enemy_pieces;

            for (int square = 0; square < 64; square++)
            {
                square_mask = Constants.bit_index_to_mask[square];

                // Is it white to move?
                if ((position.flags & Constants.flag_white_to_move) != 0UL)
                {
                    // Get masks of all pieces
                    my_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                    enemy_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;

                    // King
                    if ((position.w_king & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_K, my_pieces, enemy_pieces, true);
                    }

                    // Queen
                    if ((position.w_queen & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_Q, my_pieces, enemy_pieces, true);
                    }

                    // Rook
                    if ((position.w_rook & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_R, my_pieces, enemy_pieces, true);
                    }

                    // Bishop
                    if ((position.w_bishop & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_B, my_pieces, enemy_pieces, true);
                    }

                    // Knight
                    if ((position.w_knight & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_N, my_pieces, enemy_pieces, true);
                    }

                    // Pawn
                    if ((position.w_pawn & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_P, my_pieces, enemy_pieces, true);
                    }
                }
                else // Black to move
                {
                    // Get masks of all pieces
                    enemy_pieces = position.w_king | position.w_queen | position.w_rook | position.w_bishop | position.w_knight | position.w_pawn;
                    my_pieces = position.b_king | position.b_queen | position.b_rook | position.b_bishop | position.b_knight | position.b_pawn;

                    // King
                    if ((position.b_king & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_K, my_pieces, enemy_pieces, false);
                    }

                    // Queen
                    if ((position.b_queen & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_Q, my_pieces, enemy_pieces, false);
                    }

                    // Rook
                    if ((position.b_rook & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_R, my_pieces, enemy_pieces, false);
                    }

                    // Bishop
                    if ((position.b_bishop & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_B, my_pieces, enemy_pieces, false);
                    }

                    // Knight
                    if ((position.b_knight & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_N, my_pieces, enemy_pieces, false);
                    }

                    // Pawn
                    if ((position.b_pawn & square_mask) != 0UL)
                    {
                        GetMovesForPiece(ref moves, ref index, square, square_mask, Constants.piece_P, my_pieces, enemy_pieces, false);
                    }
                }
            }

            moves_count = index;
            return moves;
        }

        private void GetMovesForPiece(ref Move[] moves, ref int move_index, int square, ulong square_mask, int pieceType, ulong my_pieces, ulong enemy_pieces, bool white_to_play)
        {
            ulong destination;
            ulong moveflags;
            bool capture;

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
                        moveflags &= Constants.move_flag_is_capture;
                        capture = true;
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

                            // Make sure the pawns only move sideways if they are capturing
                            if (destination == (square_mask << 9) && !capture)
                            {
                                break;
                            }
                            if (destination == (square_mask << 7) && !capture)
                            {
                                break;
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
                        }
                        else
                        {
                            // Make sure the pawns don't try to move backwards
                            if (destination > square_mask)
                            {
                                break;
                            }

                            // Make sure the pawns only move sideways if they are capturing
                            if (destination == (square_mask >> 9) && !capture)
                            {
                                break;
                            }
                            if (destination == (square_mask >> 7) && !capture)
                            {
                                break;
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
                        }
                    }

                    moves[move_index++] = new Move()
                    {
                        mask_from = square_mask,
                        mask_to = destination,
                        flags = moveflags,
                        from_piece_type = pieceType
                    };

                    if (capture)
                    {
                        break;
                    }
                }
                // TODO: Break out if first element of "m" index above is 
            }
        }

        private bool CanBeCaptured(ulong square_mask, ulong my_pieces, ulong enemy_diag, ulong enemy_cross, ulong enemy_knight, ulong enemy_pawn, ulong enemy_all, bool source_is_white_piece)
        {
            ulong destination;
            int from_square = Utility.GetIndexFromMask(square_mask);

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

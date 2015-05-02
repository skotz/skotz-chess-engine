using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class BoardGenerator
    {
        public static Board FromFEN(string fen)
        {
            Board board = new Board();

            List<string> parts = fen.Split(' ').ToList();

            #region Piece Placement

            List<string> pieces = parts[0].Split('/').ToList();
            int square;
            int skip;
            for (int row = 0; row < 8; row++)
            {
                square = 0;
                foreach (char c in pieces[row].ToCharArray())
                {
                    switch (c)
                    {
                        case 'p':
                            board.b_pawn |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'P':
                            board.w_pawn |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'r':
                            board.b_rook |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'R':
                            board.w_rook |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'b':
                            board.b_bishop |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'B':
                            board.w_bishop |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'n':
                            board.b_knight |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'N':
                            board.w_knight |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'q':
                            board.b_queen |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'Q':
                            board.w_queen |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'k':
                            board.b_king |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        case 'K':
                            board.w_king |= Constants.bit_index_to_mask[(7 - square) + (7 - row) * 8];
                            break;

                        default:
                            int.TryParse(c.ToString(), out skip);
                            square += skip - 1;
                            break;
                    }
                    square++;
                }
            }

            #endregion

            #region Side To Move

            string sidetomove = parts[1];
            if (sidetomove.ToLower() == "w")
            {
                board.flags |= Constants.flag_white_to_move;
            }

            #endregion

            #region Castling Privelages

            string castling = parts[2];
            if (sidetomove.Contains("K"))
            {
                board.flags |= Constants.flag_castle_white_king;
            }
            if (sidetomove.Contains("k"))
            {
                board.flags |= Constants.flag_castle_black_king;
            }
            if (sidetomove.Contains("Q"))
            {
                board.flags |= Constants.flag_castle_white_queen;
            }
            if (sidetomove.Contains("q"))
            {
                board.flags |= Constants.flag_castle_black_queen;
            }

            #endregion

            #region En-Passant Square

            string enpassant = parts[3];
            if (enpassant == "a3")
            {
                board.en_passent_square = Constants.mask_A3;
            }
            if (enpassant == "b3")
            {
                board.en_passent_square = Constants.mask_B3;
            }
            if (enpassant == "c3")
            {
                board.en_passent_square = Constants.mask_C3;
            }
            if (enpassant == "d3")
            {
                board.en_passent_square = Constants.mask_D3;
            }
            if (enpassant == "e3")
            {
                board.en_passent_square = Constants.mask_E3;
            }
            if (enpassant == "f3")
            {
                board.en_passent_square = Constants.mask_F3;
            }
            if (enpassant == "g3")
            {
                board.en_passent_square = Constants.mask_G3;
            }
            if (enpassant == "h3")
            {
                board.en_passent_square = Constants.mask_H3;
            }
            if (enpassant == "a6")
            {
                board.en_passent_square = Constants.mask_A6;
            }
            if (enpassant == "b6")
            {
                board.en_passent_square = Constants.mask_B6;
            }
            if (enpassant == "c6")
            {
                board.en_passent_square = Constants.mask_C6;
            }
            if (enpassant == "d6")
            {
                board.en_passent_square = Constants.mask_D6;
            }
            if (enpassant == "e6")
            {
                board.en_passent_square = Constants.mask_E6;
            }
            if (enpassant == "f6")
            {
                board.en_passent_square = Constants.mask_F6;
            }
            if (enpassant == "g6")
            {
                board.en_passent_square = Constants.mask_G6;
            }
            if (enpassant == "h6")
            {
                board.en_passent_square = Constants.mask_H6;
            }

            #endregion

            #region Move Numbers

            string halfmove = parts[3];
            string fullmove = parts[4];

            int half;
            int full;
            
            int.TryParse(halfmove, out half);
            int.TryParse(fullmove, out full);

            board.half_move_number = half;
            board.move_number = full;

            #endregion

            return board;
        }

        public static Board NewStandardSetup()
        {
            return new Board()
            {
                w_king = Constants.mask_E1,
                w_queen = Constants.mask_D1,
                w_rook = Constants.mask_A1 | Constants.mask_H1,
                w_bishop = Constants.mask_C1 | Constants.mask_F1,
                w_knight = Constants.mask_B1 | Constants.mask_G1,
                w_pawn = Constants.mask_A2 | Constants.mask_B2 | Constants.mask_C2 | Constants.mask_D2 | Constants.mask_E2 | Constants.mask_F2 | Constants.mask_G2 | Constants.mask_H2,

                b_king = Constants.mask_E8,
                b_queen = Constants.mask_D8,
                b_rook = Constants.mask_A8 | Constants.mask_H8,
                b_bishop = Constants.mask_C8 | Constants.mask_F8,
                b_knight = Constants.mask_B8 | Constants.mask_G8,
                b_pawn = Constants.mask_A7 | Constants.mask_B7 | Constants.mask_C7 | Constants.mask_D7 | Constants.mask_E7 | Constants.mask_F7 | Constants.mask_G7 | Constants.mask_H7,

                flags = Constants.flag_castle_black_king | Constants.flag_castle_white_king | Constants.flag_castle_black_queen | Constants.flag_castle_white_queen | Constants.flag_white_to_move
            };
        }
    }
}

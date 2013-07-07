using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class BoardGenerator
    {
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

                flags = Constants.flag_castle_black | Constants.flag_castle_white | Constants.flag_white_to_move
            };
        }
    }
}

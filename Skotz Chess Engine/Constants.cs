using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skotz_Chess_Engine
{
    struct Constants
    {
        #region Piece Types

        public const int piece_K = 5;
        public const int piece_Q = 4;
        public const int piece_R = 3;
        public const int piece_B = 2;
        public const int piece_N = 1;
        public const int piece_P = 0;

        #endregion

        #region Directions

        public const int direction_N = 3;
        public const int direction_E = 2;
        public const int direction_S = 1;
        public const int direction_W = 0;

        #endregion

        #region Bit Index Squares

        // These are the bit indexes in a 64 bit integer for each square on the board.
        // E.G., square G2 = 9 which corresponds with 0x0000000000000100

        public const int bit_H1 = 0;
        public const int bit_G1 = 1;
        public const int bit_F1 = 2;
        public const int bit_E1 = 3;
        public const int bit_D1 = 4;
        public const int bit_C1 = 5;
        public const int bit_B1 = 6;
        public const int bit_A1 = 7;
        public const int bit_H2 = 8;
        public const int bit_G2 = 9;
        public const int bit_F2 = 10;
        public const int bit_E2 = 11;
        public const int bit_D2 = 12;
        public const int bit_C2 = 13;
        public const int bit_B2 = 14;
        public const int bit_A2 = 15;
        public const int bit_H3 = 16;
        public const int bit_G3 = 17;
        public const int bit_F3 = 18;
        public const int bit_E3 = 19;
        public const int bit_D3 = 20;
        public const int bit_C3 = 21;
        public const int bit_B3 = 22;
        public const int bit_A3 = 23;
        public const int bit_H4 = 24;
        public const int bit_G4 = 25;
        public const int bit_F4 = 26;
        public const int bit_E4 = 27;
        public const int bit_D4 = 28;
        public const int bit_C4 = 29;
        public const int bit_B4 = 30;
        public const int bit_A4 = 31;
        public const int bit_H5 = 32;
        public const int bit_G5 = 33;
        public const int bit_F5 = 34;
        public const int bit_E5 = 35;
        public const int bit_D5 = 36;
        public const int bit_C5 = 37;
        public const int bit_B5 = 38;
        public const int bit_A5 = 39;
        public const int bit_H6 = 40;
        public const int bit_G6 = 41;
        public const int bit_F6 = 42;
        public const int bit_E6 = 43;
        public const int bit_D6 = 44;
        public const int bit_C6 = 45;
        public const int bit_B6 = 46;
        public const int bit_A6 = 47;
        public const int bit_H7 = 48;
        public const int bit_G7 = 49;
        public const int bit_F7 = 50;
        public const int bit_E7 = 51;
        public const int bit_D7 = 52;
        public const int bit_C7 = 53;
        public const int bit_B7 = 54;
        public const int bit_A7 = 55;
        public const int bit_H8 = 56;
        public const int bit_G8 = 57;
        public const int bit_F8 = 58;
        public const int bit_E8 = 59;
        public const int bit_D8 = 60;
        public const int bit_C8 = 61;
        public const int bit_B8 = 62;
        public const int bit_A8 = 63;

        #endregion

        #region Bit Mask Squares

        // These are the bit masks for each square on the board.
        // E.G., to see if there is a piece on square G3 we could check ((board & mask_G3) != 0x0)

        public const ulong mask_H1 = 0x1UL;
        public const ulong mask_G1 = 0x2UL;
        public const ulong mask_F1 = 0x4UL;
        public const ulong mask_E1 = 0x8UL;
        public const ulong mask_D1 = 0x10UL;
        public const ulong mask_C1 = 0x20UL;
        public const ulong mask_B1 = 0x40UL;
        public const ulong mask_A1 = 0x80UL;
        public const ulong mask_H2 = 0x100UL;
        public const ulong mask_G2 = 0x200UL;
        public const ulong mask_F2 = 0x400UL;
        public const ulong mask_E2 = 0x800UL;
        public const ulong mask_D2 = 0x1000UL;
        public const ulong mask_C2 = 0x2000UL;
        public const ulong mask_B2 = 0x4000UL;
        public const ulong mask_A2 = 0x8000UL;
        public const ulong mask_H3 = 0x10000UL;
        public const ulong mask_G3 = 0x20000UL;
        public const ulong mask_F3 = 0x40000UL;
        public const ulong mask_E3 = 0x80000UL;
        public const ulong mask_D3 = 0x100000UL;
        public const ulong mask_C3 = 0x200000UL;
        public const ulong mask_B3 = 0x400000UL;
        public const ulong mask_A3 = 0x800000UL;
        public const ulong mask_H4 = 0x1000000UL;
        public const ulong mask_G4 = 0x2000000UL;
        public const ulong mask_F4 = 0x4000000UL;
        public const ulong mask_E4 = 0x8000000UL;
        public const ulong mask_D4 = 0x10000000UL;
        public const ulong mask_C4 = 0x20000000UL;
        public const ulong mask_B4 = 0x40000000UL;
        public const ulong mask_A4 = 0x80000000UL;
        public const ulong mask_H5 = 0x100000000UL;
        public const ulong mask_G5 = 0x200000000UL;
        public const ulong mask_F5 = 0x400000000UL;
        public const ulong mask_E5 = 0x800000000UL;
        public const ulong mask_D5 = 0x1000000000UL;
        public const ulong mask_C5 = 0x2000000000UL;
        public const ulong mask_B5 = 0x4000000000UL;
        public const ulong mask_A5 = 0x8000000000UL;
        public const ulong mask_H6 = 0x10000000000UL;
        public const ulong mask_G6 = 0x20000000000UL;
        public const ulong mask_F6 = 0x40000000000UL;
        public const ulong mask_E6 = 0x80000000000UL;
        public const ulong mask_D6 = 0x100000000000UL;
        public const ulong mask_C6 = 0x200000000000UL;
        public const ulong mask_B6 = 0x400000000000UL;
        public const ulong mask_A6 = 0x800000000000UL;
        public const ulong mask_H7 = 0x1000000000000UL;
        public const ulong mask_G7 = 0x2000000000000UL;
        public const ulong mask_F7 = 0x4000000000000UL;
        public const ulong mask_E7 = 0x8000000000000UL;
        public const ulong mask_D7 = 0x10000000000000UL;
        public const ulong mask_C7 = 0x20000000000000UL;
        public const ulong mask_B7 = 0x40000000000000UL;
        public const ulong mask_A7 = 0x80000000000000UL;
        public const ulong mask_H8 = 0x100000000000000UL;
        public const ulong mask_G8 = 0x200000000000000UL;
        public const ulong mask_F8 = 0x400000000000000UL;
        public const ulong mask_E8 = 0x800000000000000UL;
        public const ulong mask_D8 = 0x1000000000000000UL;
        public const ulong mask_C8 = 0x2000000000000000UL;
        public const ulong mask_B8 = 0x4000000000000000UL;
        public const ulong mask_A8 = 0x8000000000000000UL;

        #endregion
    }
}

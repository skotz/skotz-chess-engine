using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    struct Board
    {
        public ulong flags;

        public ulong w_king;
        public ulong w_queen;
        public ulong w_rook;
        public ulong w_bishop;
        public ulong w_knight;
        public ulong w_pawn;

        public ulong b_king;
        public ulong b_queen;
        public ulong b_rook;
        public ulong b_bishop;
        public ulong b_knight;
        public ulong b_pawn;

        public ulong en_passent_square;

        public int half_move_number;
        public int move_number;

        public ulong[] history;
        public int history_next;

        // TODO: Maintain a running hash to cut down on time
        // public ulong hash;

        public bool Equals(Board other)
        {
            return w_king == other.w_king &&
                w_queen == other.w_queen &&
                w_rook == other.w_rook &&
                w_bishop == other.w_bishop &&
                w_knight == other.w_knight &&
                w_pawn == other.w_pawn &&
                b_king == other.b_king &&
                b_queen == other.b_queen &&
                b_rook == other.b_rook &&
                b_bishop == other.b_bishop &&
                b_knight == other.b_knight &&
                b_pawn == other.b_pawn;
        }

        public bool WhiteToPlay()
        {
            return (flags & Constants.flag_white_to_move) != 0UL;
        }
    }
}

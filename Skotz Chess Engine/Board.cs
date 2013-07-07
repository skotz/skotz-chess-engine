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
    }
}

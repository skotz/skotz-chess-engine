using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skotz_Chess_Engine
{
    struct Board
    {
        ulong w_king;
        ulong w_queen;
        ulong w_rook;
        ulong w_bishop;
        ulong w_knight;
        ulong w_pawn;
        ulong flags;
    }
}

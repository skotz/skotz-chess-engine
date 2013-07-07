using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class Utility
    {
        public static int GetIndexFromMask(ulong mask)
        {
            for (int i = 0; i < 64; i++)
            {
                if (((mask >> i) & 1UL) == 1UL)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string GetMoveString(ulong from_square, ulong to_square)
        {
            return Constants.move_strings[GetIndexFromMask(from_square), GetIndexFromMask(to_square)];
        }
    }
}

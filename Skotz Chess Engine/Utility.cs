using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class Utility
    {
        public static Random Rand = new Random();

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

        /// <summary>
        /// Count the number of "1" bits in a number using lookup tables
        /// </summary>
        public static int CountBits(ulong number)
        {
            return Constants.bit_counts[number & 65535] 
                + Constants.bit_counts[(number >> 16) & 65535] 
                + Constants.bit_counts[(number >> 32) & 65535] 
                + Constants.bit_counts[(number >> 48) & 65535];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    struct Move
    {
        public ulong mask_from;
        public ulong mask_to;
        public ulong flags;
        public int from_piece_type;

        public override string ToString()
        {
            return Utility.GetMoveString(mask_from, mask_to);
        }
    }
}

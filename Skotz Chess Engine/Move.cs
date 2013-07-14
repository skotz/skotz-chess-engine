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
        public int evaluation;

        public int depth;
        public int evals;
        public bool selective;

        public string primary_variation;

        public override string ToString()
        {
            return Utility.GetMoveString(mask_from, mask_to);
        }

        public ulong GetHash()
        {
            return Constants.zobrist_pieces[0, Utility.GetIndexFromMask(mask_from)] ^ Constants.zobrist_pieces[0, Utility.GetIndexFromMask(mask_to)];
        }
    }
}

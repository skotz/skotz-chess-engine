using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skotz_Chess_Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            UnitTest test = new UnitTest();
            test.WriteBits(Constants.mask_F5);

            Console.ReadKey();
        }
    }
}

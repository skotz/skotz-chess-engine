using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Skotz_Chess_Engine
{
    class UnitTest
    {
        public UnitTest()
        {
        }

        public void WriteBits(ulong val, string file = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("0x{0:X}", val));
            for (int i = 63; i >= 0; i--)
            {
                if (((val >> i) & 0x1) == 0x1)
                {
                    sb.Append("*");
                }
                else
                {
                    sb.Append("-");
                }
                if (i % 8 == 0)
                {
                    sb.AppendLine();
                }
            }

            if (file == null)
            {
                Console.WriteLine(sb.ToString());
            }
            else
            {
                WriteDebug(sb.ToString(), file);
            }
        }

        public void WriteDebug(string text, string filename = "skotz.debug.txt")
        {
            try
            {
                using (StreamWriter w = File.AppendText(filename))
                {
                    w.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

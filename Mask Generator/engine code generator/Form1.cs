using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace engine_code_generator
{
    // This is just quick hack code for generating and testing bit masks for the real deal

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
                richTextBox1.Text += "\r\nulong[64, 6, 8, 8] movement_masks = new ulong[64, 6, 8, 8]";
                richTextBox1.Text += "\r\n{";

            for (int i = 0; i < 64; i++)
            {
                string sq = ((char)('H' - (i % 8))).ToString() + ((i / 8) + 1);
                // richTextBox1.Text += "\r\npublic const int bit_" + ((char)('H' - (i % 8))).ToString() + ((i / 8) + 1) + " = " + i + ";";
                //richTextBox1.Text += "\r\npublic const ulong mask_" + sq + " = " + string.Format("0x{0:X}", (ulong)0x1 << i) + ";";
                //richTextBox1.Text += PointFromBit(i).X + " x " + PointFromBit(i).Y + " \r\n";

                Point p = PointFromBit(i);

                ulong nullmask = 0xFFFFFFFFFFFFFFFFUL;

                ulong mask = 1UL << i; 

                //int test = BitFromPoint(p);
                //richTextBox1.Text += PointFromBit(i).X + " x " + PointFromBit(i).Y + " ? " + i + " = " + test + " \r\n";

                richTextBox1.Text += "\r\n ";
                int count;

                // [square][piecetype][direction][move#]
                // [64][6][8][8]
                // 8 directions to plan for knight moves


                richTextBox1.Text += "\r\n    { // square " + sq;

                #region rook

                richTextBox1.Text += "\r\n        { // rook ";

                richTextBox1.Text += "\r\n            { // north";
                // rook up - validated
                count = 0;
                for (int y = p.Y - 1; y > 0; y--)
                {
                    ulong n = mask << 8 * y;
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", n) + ", ";
                    count++;
                    //richTextBox1.Text += "\r\n " + string.Format("0x{0:X}", n) + ";";
                    //richTextBox1.Text += "  // " + string.Format("0x{0:X}", mask);
                }
                while (count++ < 8)
                {
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", nullmask) + ", ";
                }
                richTextBox1.Text += "\r\n            }";

                richTextBox1.Text += "\r\n            { // east";
                // rook right - validated
                count = 0;
                for (int x = p.X + 1; x <= 8; x++)
                {
                    ulong n = mask >> 9 - x;
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", n) + ", ";
                    count++;
                    //richTextBox1.Text += "\r\n " + string.Format("0x{0:X}", n) + ";";
                    //richTextBox1.Text += "  // " + string.Format("0x{0:X}", mask);
                }
                while (count++ < 8)
                {
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", nullmask) + ", ";
                }
                richTextBox1.Text += "\r\n            }";

                richTextBox1.Text += "\r\n            { // south";
                // rook down - validated
                count = 0;
                for (int y = p.Y + 1; y <= 8; y++)
                {
                    ulong n = mask >> (9 - y) * 8;
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", n) + ", ";
                    count++;
                    //richTextBox1.Text += "\r\n " + string.Format("0x{0:X}", n) + ";";
                    //richTextBox1.Text += "  // " + string.Format("0x{0:X}", mask);
                }
                while (count++ < 8)
                {
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", nullmask) + ", ";
                }
                richTextBox1.Text += "\r\n            }";

                richTextBox1.Text += "\r\n            { // west";
                // rook left - validated
                count = 0;
                for (int x = p.X - 1; x > 0; x--)
                {
                    ulong n = mask << x;
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", n) + ", ";
                    count++;
                    //richTextBox1.Text += "\r\n " + string.Format("0x{0:X}", n) + ";";
                    //richTextBox1.Text += "  // " + string.Format("0x{0:X}", mask);
                }
                while (count++ < 8)
                {
                    richTextBox1.Text += "\r\n                " + string.Format("0x{0:X}", nullmask) + ", ";
                }
                richTextBox1.Text += "\r\n            }"; // end direction

                richTextBox1.Text += "\r\n        }"; // end rook

                #endregion

                richTextBox1.Text += "\r\n    }"; // end square
            }

                richTextBox1.Text += "\r\n}";
        }

        private Point PointFromBit(int index)
        {
            return new Point(8 - (index % 8), 9 - ((index / 8) + 1));
        }

        private int BitFromPoint(Point p)
        {
            //return (9 - p.X) + (9 - p.Y) * 8 - 9;
            return 72 - p.X - 8 * p.Y;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

// Scott Clayton

namespace engine_code_generator
{
    // This is just quick hack code for generating and testing bit masks for the real deal
    // This will be a tradeoff computation. Instead of having all this code in the engine, we'll pre-compute the moves for each piece from each square for faster move generation later.

    public partial class Form1 : Form
    {
        ulong nullmask = 0xFFFFFFFFFFFFFFFFUL;

        public Form1()
        {
            InitializeComponent();
        }

        public ulong RelSquare(Point p, int relX, int relY)
        {
            Point y2 = new Point(p.X - 1, p.Y - 2);
            if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
            {
                return 1UL << BitFromPoint(y2);
            }
            else
            {
                return nullmask;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder build = new StringBuilder();

            build.Append("\r\nulong[,,,] movement_masks = new ulong[64, 6, 8, 8]");
            build.Append("\r\n{");

            string empty = "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask);
            empty += ", " + "\r\n                " + string.Format("0x{0:X}", nullmask) + ", ";
            empty = "\r\n            {" + empty + "\r\n            },";

            for (int i = 0; i < 64; i++)
            {
                string sq = ((char)('H' - (i % 8))).ToString() + ((i / 8) + 1);
                // build.Append("\r\npublic const int bit_" + ((char)('H' - (i % 8))).ToString() + ((i / 8) + 1) + " = " + i + ";";
                //build.Append("\r\npublic const ulong mask_" + sq + " = " + string.Format("0x{0:X}", (ulong)0x1 << i) + ";";
                //build.Append(PointFromBit(i).X + " x " + PointFromBit(i).Y + " \r\n";

                Point p = PointFromBit(i);
                Point y2;

                ulong mask = 1UL << i;

                //int test = BitFromPoint(p);
                //build.Append(PointFromBit(i).X + " x " + PointFromBit(i).Y + " ? " + i + " = " + test + " \r\n";

                build.Append("\r\n ");
                int count;

                // [square][piecetype][direction][move#]
                // [64][6][8][8]
                // 8 directions to plan for knight moves


                build.Append("\r\n    { // square " + sq);

                #region pawn

                build.Append("\r\n        { // pawn");

                // **************************************************************************
                build.Append("\r\n            { // up");
                count = 0;
                y2 = new Point(p.X, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                    if ((i / 8) + 1 == 2)
                    {
                        n = (1UL << BitFromPoint(y2)) << 8;
                        build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                        count++;
                    }
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // down");
                count = 0;
                y2 = new Point(p.X, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                    if ((i / 8) + 1 == 7)
                    {
                        n = (1UL << BitFromPoint(y2)) >> 8;
                        build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                        count++;
                    }
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // capture left up");
                count = 0;
                y2 = new Point(p.X - 1, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // capture right up");
                count = 0;
                y2 = new Point(p.X + 1, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************


                // **************************************************************************
                build.Append("\r\n            { // capture left down");
                count = 0;
                y2 = new Point(p.X - 1, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // capture right down");
                count = 0;
                y2 = new Point(p.X + 1, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                build.Append(empty);
                build.Append(empty);

                build.Append("\r\n        },");

                #endregion

                #region knight

                build.Append("\r\n        { // knight");

                // **************************************************************************
                // knight up left
                // **************************************************************************
                build.Append("\r\n            { // up 2 left 1");
                count = 0;
                y2 = new Point(p.X - 1, p.Y - 2);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight up right
                // **************************************************************************
                build.Append("\r\n            { // up 2 right 1");
                count = 0;
                y2 = new Point(p.X + 1, p.Y - 2);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight left up
                // **************************************************************************
                build.Append("\r\n            { // left 2 up 1");
                count = 0;
                y2 = new Point(p.X - 2, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight right up
                // **************************************************************************
                build.Append("\r\n            { // right 2 up 1");
                count = 0;
                y2 = new Point(p.X + 2, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight left down
                // **************************************************************************
                build.Append("\r\n            { // left 2 down 1");
                count = 0;
                y2 = new Point(p.X - 2, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight right down
                // **************************************************************************
                build.Append("\r\n            { // right 2 down 1");
                count = 0;
                y2 = new Point(p.X + 2, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight down left
                // **************************************************************************
                build.Append("\r\n            { // down 2 left 1");
                count = 0;
                y2 = new Point(p.X - 1, p.Y + 2);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // knight down right
                // **************************************************************************
                build.Append("\r\n            { // down 2 right 1");
                count = 0;
                y2 = new Point(p.X + 1, p.Y + 2);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                build.Append("\r\n        },"); // end knight

                #endregion

                #region bishop

                build.Append("\r\n        { // bishop ");

                // **************************************************************************
                // bishop upper left - validated
                // **************************************************************************
                build.Append("\r\n            { // north-west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y - x);
                    if (y.X <= 0 || y.Y <= 0)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop upper right - validated
                // **************************************************************************
                build.Append("\r\n            { // north-east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y - x);
                    if (y.X > 8 || y.Y <= 0)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop down left - validated
                // **************************************************************************
                build.Append("\r\n            { // south-west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y + x);
                    if (y.X <= 0 || y.Y > 8)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop down right - validated
                // **************************************************************************
                build.Append("\r\n            { // south-east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y + x);
                    if (y.X > 8 || y.Y > 8)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                build.Append(empty);
                build.Append(empty);
                build.Append(empty);
                build.Append(empty);

                build.Append("\r\n        },"); // end bishop

                #endregion

                #region rook

                build.Append("\r\n        { // rook ");

                // **************************************************************************
                build.Append("\r\n            { // north");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X, p.Y - x);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // south");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X, p.Y + x);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                build.Append(empty);
                build.Append(empty);
                build.Append(empty);
                build.Append(empty);

                build.Append("\r\n        },"); // end rook

                #endregion

                #region queen

                build.Append("\r\n        { // queen ");

                // **************************************************************************
                // bishop upper left - validated
                // **************************************************************************
                build.Append("\r\n            { // north-west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y - x);
                    if (y.X <= 0 || y.Y <= 0)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop upper right - validated
                // **************************************************************************
                build.Append("\r\n            { // north-east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y - x);
                    if (y.X > 8 || y.Y <= 0)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop down left - validated
                // **************************************************************************
                build.Append("\r\n            { // south-west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y + x);
                    if (y.X <= 0 || y.Y > 8)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                // bishop down right - validated
                // **************************************************************************
                build.Append("\r\n            { // south-east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y + x);
                    if (y.X > 8 || y.Y > 8)
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************


                // **************************************************************************
                build.Append("\r\n            { // north");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X, p.Y - x);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // east");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X + x, p.Y);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // south");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X, p.Y + x);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // west");
                count = 0;
                for (int x = 1; x < 8; x++)
                {
                    Point y = new Point(p.X - x, p.Y);
                    if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                    {
                        break;
                    }
                    ulong n = 1UL << BitFromPoint(y);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************


                build.Append("\r\n        },"); // end bishop

                #endregion

                #region king

                build.Append("\r\n        { // king ");

                // **************************************************************************
                build.Append("\r\n            { // north");
                count = 0;
                y2 = new Point(p.X, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // east");
                count = 0;
                y2 = new Point(p.X + 1, p.Y);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // south");
                count = 0;
                y2 = new Point(p.X, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // west");
                count = 0;
                y2 = new Point(p.X - 1, p.Y);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // north-west");
                count = 0;
                y2 = new Point(p.X - 1, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // north-east");
                count = 0;
                y2 = new Point(p.X + 1, p.Y - 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // south-west");
                count = 0;
                y2 = new Point(p.X - 1, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                // **************************************************************************
                build.Append("\r\n            { // sount-east");
                count = 0;
                y2 = new Point(p.X + 1, p.Y + 1);
                if (!(y2.X <= 0 || y2.Y <= 0) && !(y2.X > 8 || y2.Y > 8))
                {
                    ulong n = 1UL << BitFromPoint(y2);
                    build.Append("\r\n                " + string.Format("0x{0:X}", n) + ", ");
                    count++;
                }
                while (count++ < 8)
                {
                    build.Append("\r\n                " + string.Format("0x{0:X}", nullmask) + ", ");
                }
                build.Append("\r\n            },");
                // **************************************************************************

                build.Append("\r\n        },"); // end king

                #endregion

                build.Append("\r\n    },"); // end square
            }

            build.Append("\r\n};");
            string done = build.ToString();

            // strip off commas at the end of lines
            // it's a nasty hack, but I think it's cool - it's only for one-time generation after all...
            done = Regex.Replace(done, @"\,([^}{,]*)\}", m => m.ToString().Substring(1));

            richTextBox1.Text = done;
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

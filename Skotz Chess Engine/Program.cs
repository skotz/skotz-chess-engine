using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine((new Move() { mask_from = Constants.mask_B5, mask_to = Constants.mask_E3 }).ToString());

            //UnitTest test = new UnitTest();
            
            ////test.WriteBits(Constants.movements[Constants.bit_D4, Constants.piece_B, 1, 2]);

            //test.WriteBits(9295429630892703744ul);


            //new Game().MakeMove("a1c4");

            //Game game = new Game();
            //game.ResetBoard();

            //int count;
            //Move[] moves = game.GetAllMoves(game.board, out count);
            //for (int i = 0; i < count; i++)
            //{
            //    test.WriteBits(moves[i].mask_from);
            //    test.WriteBits(moves[i].mask_to);
            //    Console.WriteLine("-------------------------");
            //    Console.ReadKey();
            //}

            UCI engine = new UCI();
            engine.Start();

            //Console.ReadKey();
        }
    }
}

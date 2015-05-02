using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// Scott Clayton 2013

namespace Skotz_Chess_Engine
{
    class UCI
    {
        Game game;

        string engine = "Skotz";
        string version = "v0.2.1";

        bool debugFileCleared = false;
        Queue<string> testInput = new Queue<string>();

        public UCI()
        {
            StartNewGame();
        }

        public void StartNewGame()
        {
            game = new Game();
            game.ResetBoard();
        }

        public void Start()
        {
            Console.WriteLine(engine + " " + version);

            bool player = true;
            bool go = false;
            bool quit = false;

            int move_time_seconds = 10;
            int move_depth = 99;

            do
            {
                if (go)
                {
                    Move best = game.GetBestMove(move_time_seconds, move_depth);
                    game.MakeMove(best);

                    Console.WriteLine("bestmove " + best.ToString());

                    go = false;
                }

                // Wait for other player
                do
                {
                    string cmd = "";
                    
                    if (testInput.Count > 0)
                    {
                        cmd = testInput.Dequeue();
                    }
                    else                        
                    {
                        cmd = Console.ReadLine();
                    }

                    AppendToDebugFile(player, cmd);

                    try
                    {
                        char[] breakit = cmd.ToCharArray();
                        if ((breakit.Length >= 4 && breakit.Length <= 5) &&
                            (breakit[0] >= 'a' && breakit[0] <= 'h') &&
                            (breakit[2] >= 'a' && breakit[2] <= 'h') &&
                            (breakit[1] >= '1' && breakit[1] <= '8') &&
                            (breakit[3] >= '1' && breakit[3] <= '8'))
                        {
                            game.MakeMove(cmd);
                            player = !player;
                            break;
                        }

                        if (cmd == "uci")
                        {
                            Console.WriteLine("id name " + engine);
                            Console.WriteLine("id author Scott Clayton");
                            Console.WriteLine("uciok");
                        }

                        if (cmd == "isready")
                        {
                            Console.WriteLine("readyok");
                        }

                        // For debugging exact positions
                        if (cmd == "test")
                        {
                            foreach (string line in File.ReadAllLines("test.txt"))
                            {
                                testInput.Enqueue(line);
                                Console.WriteLine(">>> " + line);
                            }
                        }

                        if (cmd.StartsWith("ucinewgame"))
                        {
                            player = true;
                            game = new Game();
                            game.ResetBoard();
                        }

                        if (cmd.StartsWith("position "))
                        {
                            // Trim off the position command
                            cmd = cmd.Substring(9);

                            if (cmd.StartsWith("fen "))
                            {
                                string fen = cmd.Replace("fen ", "");

                                game = new Game();
                                player = game.LoadBoard(fen);

                                go = false;

                                // Add a series of moves to the fen notation just supplied
                                if (cmd.Contains("moves "))
                                {
                                    cmd = cmd.Substring(cmd.IndexOf("moves ") + 6);

                                    List<string> moves = cmd.ToLower().Replace("position startpos moves ", "").Split(' ').ToList();
                                    foreach (string m in moves)
                                    {
                                        game.MakeMove(m);
                                        player = !player;
                                    }
                                }
                            }

                            if (cmd.StartsWith("startpos "))
                            {
                                cmd = cmd.Replace("startpos ", "");

                                player = true;
                                game = new Game();
                                game.ResetBoard();

                                go = false;

                                if (cmd.StartsWith("moves "))
                                {
                                    List<string> moves = cmd.ToLower().Replace("moves ", "").Split(' ').ToList();
                                    foreach (string m in moves)
                                    {
                                        game.MakeMove(m);
                                        player = !player;
                                    }
                                }
                                break;
                            }
                        }

                        if (cmd == "go" || cmd.StartsWith("go "))
                        {
                            go = true;

                            if (cmd.Split(' ').Length >= 3)
                            {
                                if (cmd.Split(' ')[1] == "movetime")
                                {
                                    move_time_seconds = int.Parse(cmd.Split(' ')[2]) / 1000;
                                    move_depth = 99;
                                }
                                if (cmd.Split(' ')[1] == "depth")
                                {
                                    move_time_seconds = 999999;
                                    move_depth = int.Parse(cmd.Split(' ')[2]);
                                }
                            }

                            break;
                        }

                        if (cmd == "?")
                        {
                            Console.WriteLine("Forcing a move...");
                            // TODO
                        }

                        if (cmd == "quit")
                        {
                            quit = true;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Unknown Command: " + cmd);
                    }
                } while (!quit);
            } while (!quit);
        }

        private void AppendToDebugFile(bool player, string cmd)
        {
            for (int tries = 3; tries >= 0; tries--)
            {
                try
                {
                    StreamWriter w = new StreamWriter((player ? "W" : "B") + ".out.txt", true);
                    w.WriteLine(cmd);
                    w.Close();

                    StreamWriter a = new StreamWriter("all.out.txt", debugFileCleared);
                    a.WriteLine(cmd);
                    a.Close();
                    debugFileCleared = true;

                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
    }
}

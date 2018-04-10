
using DominoIA.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoIA
{
    class Program
    {
        static int MAX_DEGREE_PARALLEL = 2;
        static int NB_GAME_TEST = 10000;
        static int MAX_ITERATION = 10000000;
        
        static object syncObj = new object();

        static void Main(string[] args)
        {
            Console.WriteLine();
            string key="";
            
            Population population = new Population();

            population.Initialize();
            //Stopwatch st = new Stopwatch();
            //st.Start();
            
            while (key!="q")
            {
                for(int i=0;i<MAX_ITERATION;i++)
                {
                    population.Evaluate();
                    var bestPlayer = population.Classement.First().pl;
                    var players = new [] { bestPlayer, new IADummyPlayer() };
                    Dictionary<string, int> nbWin = new Dictionary<string, int>();
                    Parallel.For(0, NB_GAME_TEST, new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_PARALLEL }, k =>
                    {
                        GameIA game = new GameIA();
                        game.Initialize(6, players);
                        var winnersGame = game.Run();
                        foreach (var win in winnersGame)
                        {
                            lock (syncObj)
                            {
                                if (!nbWin.ContainsKey(win.id))
                                {
                                    nbWin[win.id] = 0;
                                }
                                nbWin[win.id] += 1;
                            }
                        }
                    });
                    Console.WriteLine("Player win rate (%) : " + (double)nbWin[bestPlayer.id]*100 / (double)NB_GAME_TEST);
                    //st.Stop();
                    //Console.WriteLine(st.ElapsedMilliseconds);
                    drawTextProgressBar(i, MAX_ITERATION);
                    population.Reproduction();
                }
                Console.WriteLine("Coninuer ? Appuyer sur (q) pour quitter");
                key = Console.ReadKey().KeyChar.ToString();


            }
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            float onechunk = 50.0f / total;
            if (progress == 1)
            {
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = 52;
                Console.Write("]"); //end
                Console.SetCursorPosition(x, y);
            }
            if (progress == total || (progress) % (total / 50) == 0)
            {
                Console.CursorTop = 0;
                //draw empty progress bar
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = 52;
                Console.Write("]"); //end
                Console.CursorLeft = 1;

                //draw filled part
                int position = 1;
                for (int i = 0; i < onechunk * progress; i++)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.CursorLeft = position++;
                    Console.Write(" ");
                }
                if (progress == total && position <= 51)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.CursorLeft = position++;
                    Console.Write(" ");
                }
                //draw unfilled part
                for (int i = position; i <= 51; i++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.CursorLeft = position++;
                    Console.Write(" ");
                }

                //draw totals
                Console.CursorLeft = 55;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
                Console.SetCursorPosition(x, y);

            }

        }
    }
}

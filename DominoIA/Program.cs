
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
        static int GENETIQUE_ITERATION = 10000;
        static int MAX_ITERATION = 10000000;
        static int NB_PLAYERS = 2;// 4 ou 3 ou 2

        private static void drawTextProgressBar(int progress, int total)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            float onechunk = 50.0f / total;
            if(progress==1)
            {
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = 52;
                Console.Write("]"); //end
                Console.SetCursorPosition(x, y);
            }
            if(progress == total || (progress)%(total/50)==0)
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
                if(progress == total && position<=51)
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
        static object syncObj = new object();
        static void Main(string[] args)
        {
            Console.WriteLine();
            string key="";
            int counter = 0;
            int gameplayed = 0;
            
            Population population = new Population();
            population.Initialize();
            Stopwatch st = new Stopwatch();
            st.Start();
            
            while (key!="q")
            {
                //for(int k=0; k< GENETIQUE_ITERATION; k++)
                Parallel.For(0, GENETIQUE_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = 3 }, k =>
                {
                    GameIA game = new GameIA();
                    var players = new Player[NB_PLAYERS];
                    lock (syncObj)
                    {
                        SelectPlayers(population, players);
                    }

                    game.Initialize(6, players);
                    var winners = game.Run();
                    foreach (var win in winners)
                    {
                        lock (syncObj)
                        {
                            if (win.name == "looser")
                            {
                                population.looserScores[Array.IndexOf(population.loosers, win)]+=1;
                            }
                            else
                            {
                                population.scores[Array.IndexOf(population.players, win)] += 1;
                            }
                        }
                        //Console.Write(win.name + " " + win.Main.Count+" ; ");
                    }
                }
                );
                counter += GENETIQUE_ITERATION;
                gameplayed += GENETIQUE_ITERATION;
                drawTextProgressBar(counter, MAX_ITERATION);
                if(counter%MAX_ITERATION==0)
                {
                    var classement = population.players.Zip(population.scores.Zip(population.played, (s, p) => p > 0 ? (double)s / (double)p : 0), (p, s) => new { pl = p, sc = s }).OrderByDescending(cp => cp.sc);

                    var topWinners = classement.Take(10).ToArray();
                    Console.WriteLine("");
                    Console.WriteLine("total game : " + gameplayed);
                    Console.WriteLine("total score last game : " + population.scores.Sum());
                    int j = 0;
                    foreach(var w in topWinners)
                    {
                        j++;
                        Console.WriteLine(j+" : " + w.pl.name + " / " + w.sc+" / gen : "+w.pl.generation);
                    }

                    st.Stop();
                    Console.WriteLine(st.ElapsedMilliseconds);
                    Console.WriteLine("Quitter ? (q)");

                    key = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();
                    counter = 0;
                }

                if (counter % GENETIQUE_ITERATION == 0)
                {

                    Console.WriteLine("total score last game : " + population.scores.Sum());
                    population.Reproduction();

                }
            }
        }

        private static void SelectPlayers( Population population, Player[] players)
        {

            var classementPl = population.players.Zip(population.scores.Zip(population.played, (s, p) => p > 0 ? (double)s / (double)p : 0), (p, s) => new { pl = p, sc = s }).OrderByDescending(cp => cp.sc).Select(p => p.pl).ToArray();
            int i = 0;
            while (players.Any(p => p == null))
            {

                if (players.Any(p => p != null && p?.name == "looser"))
                {
                    var ind = StaticRandom.Next(100);
                    var plIa = players.FirstOrDefault(p => p != null && p?.name != "looser");
                    if (plIa != null)
                    {
                        ind = Array.IndexOf(classementPl, plIa);
                        ind = StaticRandom.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                    }
                    var pl = classementPl[ind];

                    if (!players.Any(p => p?.id == pl.id))
                    {
                        players[i] = pl;
                        population.played[Array.IndexOf(population.players, pl)] += 1;
                        i++;
                    }
                }
                else if ((i == NB_PLAYERS - 1 || StaticRandom.NextDouble() >= 0.5) )
                {
                    var ind = StaticRandom.Next(10);
                    var pl = population.loosers[ind];
                    if (!players.Any(p => p?.id == pl.id))
                    {
                        players[i] = pl;
                        population.looserPlayed[ind] += 1;
                        i++;
                    }
                }
                else
                {
                    var ind = StaticRandom.Next(100);
                    var plIa = players.FirstOrDefault(p => p != null && p?.name != "looser");
                    if (plIa != null)
                    {
                        ind = Array.IndexOf(classementPl, plIa);
                        ind = StaticRandom.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                    }
                    var pl = classementPl[ind];

                    if (!players.Any(p => p?.id == pl.id))
                    {
                        players[i] = pl;
                        population.played[Array.IndexOf(population.players, pl)] += 1;
                        i++;
                    }
                }
                //if (players.Any(p => p != null && p?.name != "looser"))
                //{
                //    var ind = rnd.Next(10);
                //    var pl = population.loosers[ind];
                //    if (!players.Any(p => p?.id == pl.id))
                //    {
                //        players[i] = pl;
                //        population.looserPlayed[ind] += 1;
                //        i++;
                //    }
                //}
                //else if (i == NB_PLAYERS - 1 || rnd.NextDouble() >= 0.5)
                //{
                //    var ind = rnd.Next(100);
                //    var pl = population.players[ind];

                //    if (!players.Any(p => p?.id == pl.id))
                //    {
                //        players[i] = pl;
                //        population.played[ind] += 1;
                //        i++;
                //    }
                //}
                //else
                //{
                //    var ind = rnd.Next(10);
                //    var pl = population.loosers[ind];
                //    if (!players.Any(p => p?.id == pl.id))
                //    {
                //        players[i] = pl;
                //        population.looserPlayed[ind] += 1;
                //        i++;
                //    }
                //}
            }
        }
    }
}

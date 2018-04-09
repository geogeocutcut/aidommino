
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
        static int GAME_ITERATION = 50;
        static int GENETIQUE_ITERATION = 10000;
        static int NB_GAME_TEST = 10000;
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
                Parallel.For(0, GENETIQUE_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = 2 }, k =>
                {
                    var players = new Player[NB_PLAYERS];
                    var nbWin = new int[4];
                    lock (syncObj)
                    {
                        SelectPlayers(population, players);
                    }
                    for(int gameCounter=0; gameCounter< GAME_ITERATION; gameCounter++)
                    {
                        GameIA game = new GameIA();
                        game.Initialize(6, players);
                        var winnersGame = game.Run();
                        foreach (var win in winnersGame)
                        {

                            nbWin[Array.IndexOf(players, win)] += 1;
                            //Console.Write(win.name + " " + win.Main.Count+" ; ");
                        }
                    }

                    var winners = players.Zip(nbWin, (p, n)=>new { pl = p, nbW = n });
                    var nbWinMax = winners.Max(x => x.nbW);
                    foreach (var win in winners.Where(x=>x.nbW==nbWinMax))
                    {
                        lock (syncObj)
                        {
                            if (win.pl.name == "looser")
                            {
                                population.looserScores[Array.IndexOf(population.loosers, win.pl)] += 1;
                            }
                            else
                            {
                                population.scores[Array.IndexOf(population.players, win.pl)] += 1;
                            }
                        }
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

                    //Console.WriteLine("total score last game : " + population.scores.Sum());
                    var bestplayer = population.Reproduction();
                    var players = new Player[] { bestplayer, new IADummyPlayer() };
                    var nbWin = new int[2];
                    Parallel.For(0, NB_GAME_TEST, new ParallelOptions { MaxDegreeOfParallelism = 2 }, k =>
                    {
                        GameIA game = new GameIA();
                        game.Initialize(6, players);
                        var winnersGame = game.Run();
                        lock (syncObj)
                        {
                            foreach (var win in winnersGame)
                            {
                                nbWin[Array.IndexOf(players, win)] += 1;
                                //Console.Write(win.name + " " + win.Main.Count+" ; ");
                            }
                        }
                    });
                    Console.WriteLine("Rate Win Best Player : "+ (double)nbWin[0]/(double)NB_GAME_TEST);
                }
            }
        }

        private static void SelectPlayers( Population population, Player[] players)
        {
            population.UpdateClassement();
            var classementPl = population.classement;
            int i = 0;
            while (players.Any(p => p == null))
            {

                if (players.Any(p => p != null && p?.name == "looser"))
                {
                    var ind = StaticRandom.Next(100);
                    var plIa = players.FirstOrDefault(p => p != null && p?.name != "looser");
                    if (plIa != null)
                    {
                        ind = Array.IndexOf(classementPl, classementPl.First(x=>x.pl==plIa));
                        ind = StaticRandom.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                    }
                    var pl = classementPl[ind].pl;

                    if (!players.Any(p => p?.id == pl.id))
                    {
                        players[i] = pl;
                        population.played[Array.IndexOf(population.players, pl)] += 1;
                        i++;
                    }
                }
                else if ((i == NB_PLAYERS - 1 || StaticRandom.NextDouble() >= 0.5) && NB_PLAYERS>2)
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
                        ind = Array.IndexOf(classementPl,classementPl.First(x => x.pl == plIa));
                        ind = StaticRandom.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                    }
                    var pl = classementPl[ind].pl;

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

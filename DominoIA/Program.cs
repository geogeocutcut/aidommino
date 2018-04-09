
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
        static int GAME_ITERATION = 100;
        static int GENETIQUE_ITERATION = 4000;
        static int NB_GAME_TEST = 10000;
        static int MAX_ITERATION = 10000000;
        static int NB_PLAYERS = 2;// 4 ou 3 ou 2
        static HashSet<string> mathPlayed = new HashSet<string>();


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
                mathPlayed = new HashSet<string>();
                for (int k=0; k< GENETIQUE_ITERATION; k++)
                //Parallel.For(0, GENETIQUE_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = 2 }, k =>
                {
                    var players = new ClassementItem[NB_PLAYERS];
                    var nbWin = new Dictionary<Player, int>();
                    SelectPlayers(population, players);
                    foreach (var pl in players)
                    {
                        nbWin[pl.pl] = 0;
                    }
                    

                    Parallel.For(0, GAME_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_PARALLEL }, gameCounter =>
                    {
                        GameIA game = new GameIA();
                        game.Initialize(6, players.Select(x=>x.pl).ToArray());
                        var winnersGame = game.Run();
                        foreach (var win in winnersGame)
                        {
                            lock (syncObj)
                            {
                                nbWin[win] += 1;
                            }
                        }
                    });

                    var winners = nbWin.GroupBy(x => x.Value).OrderByDescending(x => x.Key).First();
                    foreach(var win in winners)
                    {
                        population.classement[win.Key].point += 1;
                    }

                }
                //);
                counter += GENETIQUE_ITERATION;
                gameplayed += GENETIQUE_ITERATION;
                drawTextProgressBar(counter, MAX_ITERATION);


                if (counter % GENETIQUE_ITERATION == 0)
                {

                    //Console.WriteLine("total score last game : " + population.scores.Sum());
                    var bestplayer = population.Classement.First().pl ;
                    var players = new Player[] { bestplayer, new IADummyPlayer() };
                    var nbWin = new int[2];
                    Parallel.For(0, NB_GAME_TEST, new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_PARALLEL }, k =>
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

                    population.Reproduction();
                }

                if (counter % MAX_ITERATION == 0)
                {


                    Console.WriteLine(st.ElapsedMilliseconds);
                    Console.WriteLine("Quitter ? (q)");

                    key = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();
                    counter = 0;
                }

            }
        }
        

        private static void SelectPlayers( Population population, ClassementItem[] players)
        {
            var classementPl = population.Classement.ToArray();

            var matchPlay = "";
            while (mathPlayed.Contains(matchPlay) || matchPlay == "")
            {
                int i = 0;
                int ind = 0;
                matchPlay = "";
                for (int j=0;j<players.Length; j++)
                {
                    players[j] = null;
                }
                while (players.Any(p => p == null))
                {
                    if (!players.Any(p => p != null))
                    {
                        ind = StaticRandom.Next(100);
                        players[i] = classementPl[ind];
                        i++;
                        matchPlay = classementPl[ind].pl.id;
                    }

                    var indOpponent = StaticRandom.Next(Math.Max(0, ind - 10), Math.Min(100, ind + 10));
                    var pl = classementPl[indOpponent];
                    if (!players.Any(p => p?.pl.id == pl.pl.id))
                    {
                        players[i] = pl;
                        i++;
                        matchPlay += pl.pl.id;
                    }
                }
            }
            mathPlayed.Add(matchPlay);
            foreach(var pl in players)
            {
                population.classement[pl.pl].played += 1;
            }
        }
    }
}

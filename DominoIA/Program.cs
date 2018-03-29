
using DominoIA.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoIA
{
    class Program
    {
        static int GENETIQUE_ITERATION = 10000;
        static int MAX_ITERATION = 1000000;
        static int NB_PLAYERS = 4;// 4 ou 2

        private static void drawTextProgressBar(int progress, int total)
        {

            float onechunk = 50.0f / total;
            if(progress==1)
            {
                Console.CursorLeft = 0;
                Console.Write("["); //start
                Console.CursorLeft = 52;
                Console.Write("]"); //end
            }
            if(progress == total || (progress)%(total/50)==0)
            {
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

            }

        }

        static void Main(string[] args)
        {
            string key="";
            int counter = 0;
            int gameplayed = 0;

            Random rnd = new Random();
            Population population = new Population();
            population.Initialize();
           

            while (key!="q")
            {
                counter += 1;
                gameplayed += 1;
                GameIA game = new GameIA();
                var players=new Player[NB_PLAYERS];
                int i = 0;
                var classementPl = population.players.Zip(population.scores.Zip(population.played, (s, p) => p > 0 ? (double)s / (double)p : 0), (p, s) => new { pl = p, sc = s }).OrderByDescending(cp => cp.sc).Select(p => p.pl).ToArray();

                while (players.Any(p=>p==null))
                {

                    if (players.Any(p => p != null && p?.name == "looser"))
                    {
                        var ind = rnd.Next(100);
                        var plIa = players.FirstOrDefault(p => p != null && p?.name != "looser");
                        if (plIa != null)
                        {
                            ind = Array.IndexOf(classementPl, plIa);
                            ind = rnd.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                        }
                        var pl = classementPl[ind];

                        if (!players.Any(p => p?.id == pl.id))
                        {
                            players[i] = pl;
                            population.played[Array.IndexOf(population.players,pl)] += 1;
                            i++;
                        }
                    }
                    else if ((i == NB_PLAYERS - 1  || rnd.NextDouble() >= 0.5 ) && i > 1)
                    {
                        var ind = rnd.Next(10);
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
                        var ind = rnd.Next(100);
                        var plIa = players.FirstOrDefault(p => p != null && p?.name != "looser");
                        if (plIa != null)
                        {
                            ind = Array.IndexOf(classementPl, plIa);
                            ind = rnd.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
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

                game.Initialize(6, players);
                var winners = game.Run();
                foreach (var win in winners)
                {
                    if(win.name=="looser")
                    {
                        population.looserScores[Array.IndexOf(population.loosers, win)] += 1;
                    }
                    else
                    {
                        population.scores[Array.IndexOf(population.players, win)] += 1;
                    }
                    //Console.Write(win.name + " " + win.Main.Count+" ; ");
                }
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
                    Console.WriteLine("Quitter ? (q)");

                    key = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();
                    counter = 0;
                }

                if (counter % GENETIQUE_ITERATION == 0)
                {
                    population.Reproduction();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class ClassementItem
    {
        public Player pl;
        public double score
        {
            get { return played > 0 ? won / played : 0; }
        }
        public double won;
        public double played;
        public int classement;

        public List<Tuple<string,int>> wonGames = new List<Tuple<string, int>>();
        public List<Tuple<string, int>> lostGames = new List<Tuple<string, int>>();
    }
    public class Population
    {
        static object syncObj= new object();
        static int MAX_DEGREE_PARALLEL = 2;
        static int GAME_ITERATION = 5;
        static int GENETIQUE_ITERATION = 4000;
        static int NB_PLAYERS = 2;// 4 ou 3 ou 2

        static int CLASSEMENT_MODE = 2;// 1 position classement ou 2 score

        Random rnd = new Random();
        public Player[] players = new Player[100];
        public Dictionary<string, ClassementItem> classement = new Dictionary<string, ClassementItem>();

        public IEnumerable<ClassementItem>  Classement
        {
            get {
                if(CLASSEMENT_MODE==1)
                    return classement.OrderBy(x => x.Value.classement).Select(x => x.Value);
                else
                    return classement.OrderByDescending(x => x.Value.score).Select(x => x.Value); 
            }
        }

        public void Initialize()
        {
            for(int i=0; i<players.Count();i++)
            {
                players[i] = new IAHardPlayer
                {
                    name = "player " + i,
                    coeff_double = rnd.NextDouble() * 10,
                    coeff_valeur = rnd.NextDouble() * 10,
                    coeff_div = rnd.NextDouble() * 10,
                    coeff_bloq = rnd.NextDouble() * 10,
                    coeff_incertitude = rnd.NextDouble() * 10,
                    indice_mutuabilite = (rnd.NextDouble() - 0.5)
                };
            }

            InitializeClassement();
        }
        

        public void Reproduction()
        {
            var clsmt = Classement;
            var winners = clsmt.Take(10).Select(x=>x.pl);
            var loosers = clsmt.Skip(90).Select(x => x.pl);

            var iterations = Math.Min(winners.Count(), loosers.Count());   


            for (int i = 0; i < iterations; i++)
            {
                var indloos = loosers.Count() - 1 - i;
                if (indloos>0 && loosers.ElementAt(indloos) is IAPlayer)
                {
                    var winner = (IAPlayer)winners.ElementAt(i);
                    var player = (IAPlayer)loosers.ElementAt(indloos);
                    var mutabilite = winner.indice_mutuabilite;
                    player.coeff_valeur = mutation(winner.coeff_valeur, mutabilite);
                    player.coeff_double = mutation(winner.coeff_double, mutabilite);
                    player.coeff_div = mutation(winner.coeff_div, mutabilite);
                    player.coeff_bloq = mutation(winner.coeff_bloq, mutabilite);
                    player.coeff_incertitude = mutation(winner.coeff_incertitude, mutabilite);
                    player.indice_mutuabilite = mutation(winner.indice_mutuabilite, mutabilite);
                    player.generation = winner.generation + 1;
                    player.name = winner.name;
                }
            }
            InitializeClassement() ;
        }

        private void InitializeClassement()
        {
            classement = new Dictionary<string, ClassementItem>();
            int i = 0;
            foreach (var p in players)
            {
                i++;
                classement[p.id] = new ClassementItem { pl = p ,classement=i};
            }
        }

        public double mutation(double coeff, double mutabilite)
        {
            return coeff * (1 + mutabilite * (2 * rnd.NextDouble() - 1));
        }

        public void Evaluate()
        {
            Dictionary<string, HashSet<string>> playedMatches = new Dictionary<string, HashSet<string>>();
            for(int i=0;i< GENETIQUE_ITERATION; i++)
            {
                // Selection de celui qui à fait le moins de match
                var gamePlayers = SelectPlayers(playedMatches);

                // 100 games
                Dictionary<string, int> nbWin = new Dictionary<string, int>();
                Parallel.For(0, GAME_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_PARALLEL }, k =>
                {
                    GameIA game = new GameIA();
                    game.Initialize(6, gamePlayers.Select(x=>x.pl).ToArray());
                    var winnersGame = game.Run();
                    foreach (var win in winnersGame)
                    {
                        lock (syncObj)
                        {
                            if(!nbWin.ContainsKey(win.id))
                            {
                                nbWin[win.id] =0;
                            }
                            nbWin[win.id] += 1;
                        }
                    }
                });

                // Mise à jour du classement
                var winners = nbWin.GroupBy(x => x.Value).OrderByDescending(x => x.Key).First();
                var classMin = classement.Where(x => nbWin.ContainsKey(x.Key)).Min(x => x.Value.classement);

                var wins = winners.Select(x => x.Key);
                foreach (var win in winners)
                {
                    classement[win.Key].won += 1;
                    if(classMin != classement[win.Key].classement)
                    {
                        var elmtToMove = Classement.Where(x => x.classement >= classMin && x.classement < classement[win.Key].classement);
                        foreach(var classt in elmtToMove)
                        {
                            classt.classement += 1;
                        }
                        classement[win.Key].classement = classMin;
                    }
                    foreach(var p in nbWin.Where(x=> !wins.Contains(x.Key)))
                    {
                        classement[win.Key].wonGames.Add(new Tuple<string,int>(p.Key, win.Value));
                        classement[p.Key].lostGames.Add(new Tuple<string, int>(win.Key, p.Value));
                    }
                }
            }
        }

        private ClassementItem[] SelectPlayers(Dictionary<string, HashSet<string>> playedMatches)
        {
            var clsst = Classement.ToArray();
            var player1Str = "";
            var player2Str = "";
            var playersTmp = new ClassementItem[NB_PLAYERS];
            while (!playedMatches.ContainsKey(player1Str) || (playedMatches.ContainsKey(player1Str)  && playedMatches[player1Str].Contains(player2Str)))
            {
                int i = 0;
                player1Str = "";
                player2Str = "";
                playersTmp = new ClassementItem[NB_PLAYERS];
                int ind = 0;
                while (playersTmp.Any(x => x == null))
                {
                    if(!playersTmp.Any(x=>x!=null))
                    {
                        var player1 = classement.OrderBy(x => x.Value.played).First().Value;
                        playersTmp[i] = player1;
                        player1Str = player1.pl.id;
                        if (!playedMatches.ContainsKey(player1Str))
                        {
                            playedMatches[player1Str] = new HashSet<string>();
                        }
                        i++;
                        ind = Array.IndexOf(clsst, player1);
                    }
                    else
                    {
                        var ind2 = -1;
                        while (ind2 < 0 || ind2 == ind)
                        {
                            ind2 = rnd.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                        }
                        //var player2 = classement.Where(x => !playedMatches[player1Str].Contains(x.Value.pl.id) && x.Value.pl.id!=player1Str).OrderBy(x => Math.Abs(x.Value.score-classement[player1Str].score)).First().Value;
                        var player2 = clsst[ind2];
                        playersTmp[i] = player2;
                        player2Str = player2.pl.id;
                        if (!playedMatches.ContainsKey(player2Str))
                        {
                            playedMatches[player2Str] = new HashSet<string>();
                        }
                        i++;
                    }
                }
            }
            //playedMatches[player1Str].Add(player2Str);
            //playedMatches[player2Str].Add(player1Str);

            for (int i=0;i< playersTmp.Length;i++)
            {
                classement[playersTmp[i].pl.id].played += 1;
            }

            return playersTmp;
        }
        
    }
}

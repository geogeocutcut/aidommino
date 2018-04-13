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

        public List<Tuple<string,int>> wonGames = new List<Tuple<string, int>>();
        public List<Tuple<string, int>> lostGames = new List<Tuple<string, int>>();
    }
    public class Population
    {
        public static object syncObj= new object();
        static int MAX_DEGREE_PARALLEL = 2;
        static int GAME_ITERATION = 49;
        static int GENETIQUE_ITERATION = 2000;
        static int NB_PLAYERS = 2;// 4 ou 3 ou 2

        static int CLASSEMENT_MODE = 2;// 1 position classement ou 2 score

        Random rnd = new Random();
        public Player[] players = new Player[100];
        public Dictionary<string, ClassementItem> classement = new Dictionary<string, ClassementItem>();

        

        

        public IEnumerable<ClassementItem>  Classement
        {
            get {
                return classement.OrderByDescending(x => x.Value.score).Select(x => x.Value); 
            }
        }

        public void Initialize()
        {
            for(int i=0; i<players.Count();i++)
            {
                players[i] = new IAMediumPlayer
                {
                    name = "player " + i,
                    coeff_double = rnd.NextDouble() * 50,
                    coeff_valeur = rnd.NextDouble() * 50,
                    coeff_div = rnd.NextDouble() * 50,
                    coeff_bloq = rnd.NextDouble() * 50,
                    coeff_incertitude = rnd.NextDouble() * 50,
                    coeff_played = rnd.NextDouble() * 50,
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
                classement[p.id] = new ClassementItem { pl = p};
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
                foreach (var p in gamePlayers)
                {
                    nbWin[p.pl.id] = 0;
                }
                Parallel.For(0, GAME_ITERATION, new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_PARALLEL }, k =>
                {
                    GameIA game = new GameIA(50,6, gamePlayers.Select(p=>p.pl).ToArray());
                    var winnersGame = game.Run();
                    var winnerlist = winnersGame.Select(w => w.id);
                    lock (syncObj)
                    {
                        foreach (var win in winnersGame)
                        {
                            nbWin[win.id] += 1;
                        }
                    }
                });

                // Mise à jour du classement
                var winners = nbWin.GroupBy(x => x.Value).OrderByDescending(x => x.Key).First();
                
                var wins = winners.Select(x => x.Key);
                foreach (var win in winners)
                {
                    classement[win.Key].won += 1;
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
            var playersTmp = new ClassementItem[NB_PLAYERS];
            int i = 0;
            int ind = 0;
            while (playersTmp.Any(x => x == null))
            {
                if(!playersTmp.Any(x=>x!=null))
                {
                    var player1 = classement.OrderBy(x => x.Value.played).First().Value;
                    playersTmp[i] = player1;
                    i++;
                    ind = Array.IndexOf(clsst, player1);
                }
                else
                {
                    var ind2 = StaticRandom.Next(Math.Max(0, ind - 5), Math.Min(100, ind + 5));
                    var player2 = clsst[ind2];
                    //var player2 = clsst.OrderBy(p=>p.score- clsst[ind].score)
                    if (!playersTmp.Any(x => x?.pl?.id== player2.pl.id))
                    {
                        playersTmp[i] = player2;
                        i++;
                    }
                }
            }

            for (int j=0;j< playersTmp.Length;j++)
            {
                classement[playersTmp[j].pl.id].played += 1;
            }

            return playersTmp;
        }
        
    }
}

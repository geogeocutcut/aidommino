using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DominoIA.Game
{
    public static class StaticRandom
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
            return random.Value.Next();
        }
        public static int Next(int maxvalue)
        {
            return random.Value.Next(maxvalue);
        }


        public static int Next(int minvalue,int maxvalue)
        {
            return random.Value.Next(minvalue,maxvalue);
        }

        public static double NextDouble()
        {
            return random.Value.NextDouble();
        }
    }

    public class GameIA
    {

        
        public Dictionary<Player,Dictionary<Domino, Dictionary<Player, DominoProbabilite>>> dominoProbabilites = new Dictionary<Player, Dictionary<Domino, Dictionary<Player, DominoProbabilite>>>();
        public Dictionary<Player, Dictionary<Player, Dictionary<Domino, DominoProbabilite>>> playerProbabilites = new Dictionary<Player, Dictionary<Player, Dictionary<Domino, DominoProbabilite>>>();
        public Dictionary<Player, Dictionary<Domino, DominoProbabilite>> piocheProbabilites = new Dictionary<Player, Dictionary<Domino, DominoProbabilite>>();

        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, HashSet<Domino>> mains = new Dictionary<string, HashSet<Domino>>();

        public Dictionary<string, int> playersPioche = new Dictionary<string, int>();

        public List<Domino> Pioche = new List<Domino>();
        public List<Domino> Dominos = new List<Domino>();
        public List<Action> actionsHistory = new List<Action>();

        public List<int> PlayedDominos = new List<int>();
        
        public int nbDominoMainInitial;
        public int nbDominoPiocheInitial;

        public GameIA() 
        {
            
        }

        public void Initialize(int maxValue, Player[] playersTmp)
        {
            Pioche = new List<Domino>();
            Dominos = new List<Domino>();
            for (int i = 0; i <= maxValue; i++)
            {
                for (int j = i; j <= maxValue; j++)
                {
                    var d = new Domino { Values = new int[2] { i, j } };
                    Pioche.Add(d);
                    Dominos.Add(d);
                }
            }
            nbDominoMainInitial = playersTmp.Length > 2 ? 6 : 7;
            nbDominoPiocheInitial = 28%nbDominoMainInitial;

            foreach (var pl in playersTmp)
            {
                players.Add(pl.id,pl);
                mains[pl.id] = new HashSet<Domino>();
                playersPioche[pl.id] = 0;
            }

            foreach(var p in players)
            {
                p.Value.Initialize(this);
            }
        }

        public IEnumerable<Player> Run()
        {
            int i = 0;
            int index = -1;
            int p = 0;
            string[] actions = new string[players.Count];
            Action action;
            var playersTmp = players.Values.ToArray();
            while (true)
            {
                if (i == 0)
                {
                    var firstAction = mains.Where(p1=> p1.Value.Any(d => d.IsDouble())).Select(pl => new { player = players[pl.Key], domino = pl.Value.Where(d => d.IsDouble())
                        .OrderByDescending(d => d.GetValue())
                        .FirstOrDefault() })
                        .OrderByDescending(a => a.domino?.GetValue())
                        .FirstOrDefault();
                    if (firstAction == null)
                    {
                        firstAction = mains.Select(pl => new { player = players[pl.Key], domino = pl.Value.OrderByDescending(d => d.GetValue()).FirstOrDefault() }).OrderByDescending(a => a.domino.GetValue()).FirstOrDefault();
                    }
                    index = Array.IndexOf(playersTmp, firstAction.player);
                    p = index;
                    action=firstAction.player.StartGame(this,firstAction.domino);
                    action.player = firstAction.player;
                }
                else
                {
                    p = (i + index) % players.Count;
                    action = players.ElementAt(p).Value.NextAction(this);
                    action.player = players.ElementAt(p).Value;
                    actions[p] = action.name;
                    if (mains[playersTmp[p].id].Count == 0)
                    {
                        return new Player[] { playersTmp[p] };
                    }
                    if (!actions.Any(a => a != "passe"))
                    {
                        var result = mains.Select(x => new { pl = players[x.Key], valeur = x.Value.Sum(d => d.GetValue()) })
                            .GroupBy(v=>v.valeur)
                            .OrderBy(v=>v.Key);
                        return result.First().Select(v=>v.pl);
                    }
                }

                actionsHistory.Add(action);
                if(action.name=="pioche")
                {
                    playersPioche[action.player.id] += 1;
                }
                if (action.name=="domino")
                {
                    Dominos.Remove(action.domino);
                }
                for(int p2=0;p2<playersTmp.Length;p2++)
                {
                    if (p != p2)
                    {
                        playersTmp[p2].UpdateState(this,playersTmp[p], action);
                    }
                }

                i++;
            }
        }
    }
}

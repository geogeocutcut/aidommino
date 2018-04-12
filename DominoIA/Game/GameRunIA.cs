using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DominoIA.Game
{

    public class GameRunIA
    {

        private int game_endscore = 0;
        public Dictionary<Player,Dictionary<Domino, Dictionary<Player, DominoProbabilite>>> dominoProbabilites = new Dictionary<Player, Dictionary<Domino, Dictionary<Player, DominoProbabilite>>>();
        public Dictionary<Player, Dictionary<Player, Dictionary<Domino, DominoProbabilite>>> playerProbabilites = new Dictionary<Player, Dictionary<Player, Dictionary<Domino, DominoProbabilite>>>();
        public Dictionary<Player, Dictionary<Domino, DominoProbabilite>> piocheProbabilites = new Dictionary<Player, Dictionary<Domino, DominoProbabilite>>();

        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, int> scores = new Dictionary<string, int>();
        public Dictionary<string, HashSet<Domino>> mains = new Dictionary<string, HashSet<Domino>>();

        public Dictionary<string, int> playersPioche = new Dictionary<string, int>();

        public List<Domino> Pioche = new List<Domino>();
        public List<Domino> Dominos = new List<Domino>();
        public List<Action> actionsHistory = new List<Action>();

        public List<int> PlayedDominos = new List<int>();
        
        public int nbDominoMainInitial;
        public int nbDominoPiocheInitial;

        public GameRunIA(int endscore) 
        {
            game_endscore = endscore;
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

        public IEnumerable<KeyValuePair<string, int>> Run()
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
                }

                actionsHistory.Add(action);
                if (mains[playersTmp[p].id].Count == 0 || !actions.Any(a => a != "passe"))
                {
                    return mains.Select(x=>new KeyValuePair<string,int>( x.Key, x.Value.Sum(d => d.GetValue())));
                }
                if (action.name=="pioche")
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
                if (action.name != "pioche")
                {
                    i++;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoIA.Game
{
    public class GameIA
    {
        public Player[] players = new Player[4];
        public List<Domino> Pioche = new List<Domino>();
        public List<Domino> Dominos = new List<Domino>();

        public List<int> PlayedDominos = new List<int>();
        public static Random rnd = new Random();
        public GameIA()
        {
            
        }

        public void Initialize(int maxValue, Player[] playersTmp)
        {
            for (int i = 0; i <= maxValue; i++)
            {
                for (int j = i; j <= maxValue; j++)
                {
                    var d = new Domino { Values = new int[2] { i, j } };
                    Pioche.Add(d);
                    Dominos.Add(d);
                }
            }

            players = playersTmp;

            foreach(var p in players)
            {
                p.Initialize(this);
            }
        }

        public IEnumerable<Player> Run()
        {
            int i = 0;
            int index = -1;
            int p = 0;
            string[] actions = new string[4];
            Action action;
            while (true)
            {
                if (i == 0)
                {
                    var firstAction = players.Where(p1=> p1.Main.Any(d => d.IsDouble())).Select(pl => new { player = pl, domino = pl.Main.Where(d => d.IsDouble())
                        .OrderByDescending(d => d.GetValue())
                        .FirstOrDefault() })
                        .OrderByDescending(a => a.domino?.GetValue())
                        .FirstOrDefault();
                    if (firstAction == null)
                    {
                        firstAction = players.Select(pl => new { player = pl, domino = pl.Main.OrderByDescending(d => d.GetValue()).FirstOrDefault() }).OrderByDescending(a => a.domino.GetValue()).FirstOrDefault();
                    }
                    index = Array.IndexOf(players, firstAction.player);
                    p = index;
                    action=firstAction.player.StartGame(firstAction.domino);
                }
                else
                {
                    p = (i + index) % 4;
                    action = players[p].NextAction();
                    actions[p] = action.name;
                    if (players[p].Main.Count == 0)
                    {
                        return new Player[] { players[p] };
                    }
                    if (!actions.Any(a => a != "passe"))
                    {
                        return players
                            .GroupBy(x => x.Main.Sum(d => d.GetValue()))
                            .OrderBy(x => x.Key)
                            .First();
                    }
                }
                if(action.name=="domino")
                {
                    Dominos.Remove(action.domino);
                }
                for (int p2 = 0; p2 < players.Length; p2++)
                {
                    if (p != p2)
                    {
                        players[p2].UpdateState(players[p], action);
                    }
                }

                i++;
            }
        }
    }
}

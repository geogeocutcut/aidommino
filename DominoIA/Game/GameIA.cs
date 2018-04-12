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
        private int max_value_domino = 6;
        private int game_endscore = 0;
        
        public Dictionary<string, Player> players = new Dictionary<string, Player>();
        public Dictionary<string, int> scores = new Dictionary<string, int>();
        

        public GameIA(int endscore,int max_value, Player[] playersTmp) 
        {
            game_endscore = endscore;
            max_value_domino = max_value;
            foreach (var pl in playersTmp)
            {
                scores[pl.id] = 0;
                players.Add(pl.id, pl);
            }
        }
        
        public IEnumerable<Player> Run()
        {
            while(scores.DefaultIfEmpty().Max(x=>x.Value)< game_endscore)
            {
                GameRunIA game = new GameRunIA(game_endscore);
                game.Initialize(6, players.Select(x => x.Value).ToArray());
                var result = game.Run();
                var winner = result.Min(x => x.Value);
                foreach(var r in result)
                {
                    scores[r.Key] += r.Value;
                    lock(Population.syncObj)
                    {
                        if (r.Value == winner)
                        {
                            players[r.Key].wonGames.Add(game);
                        }
                        else
                        {
                            players[r.Key].lostGames.Add(game);
                        }
                    }
                }
            }
            var winners = scores.GroupBy(x => x.Value).OrderBy(x => x.Key).First().Select(x => x.Key);
            return players.Where(x => winners.Contains(x.Key)).Select(x => x.Value);
        }
    }
}

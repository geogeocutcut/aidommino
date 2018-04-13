using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public abstract class Player
    {

        //public List<GameRunIA> wonGames = new List<GameRunIA>();
        //public List<GameRunIA> lostGames = new List<GameRunIA>();
        public string id=Guid.NewGuid().ToString();
        public string name;
        public int generation;

        public virtual void Initialize(GameRunIA game)
        {
            var main = game.mains[this.id];

            while (main.Count < game.nbDominoMainInitial)
            {
                var index = StaticRandom.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                main.Add(domino);
            }

            FinalizeInitialisation(game);
        }

        public virtual void FinalizeInitialisation(GameRunIA game)
        {

        }

        public virtual void UpdateState(GameRunIA game, Player enemy, Action action)
        {

        }
        

        public abstract Action NextAction(GameRunIA game);


        public virtual Action StartGame(GameRunIA game, Domino domino)
        {
            var main = game.mains[this.id];
            game.PlayedDominos.AddRange(domino.Values);
            main.Remove(domino);
            return new Action { name = "domino", domino = domino };

        }

        public virtual void PrintDescription()
        {
            Console.WriteLine("Player basic : " + this.name + " / " + this.id);
        }
    }
}

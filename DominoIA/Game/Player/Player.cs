using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public abstract class Player
    {

        public string id=Guid.NewGuid().ToString();
        public string name;
        public int generation;

        public virtual void Initialize(GameIA game)
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

        public virtual void FinalizeInitialisation(GameIA game)
        {

        }

        public virtual void UpdateState(GameIA game, Player enemy, Action action)
        {

        }
        

        public abstract Action NextAction(GameIA game);


        public virtual Action StartGame(GameIA game, Domino domino)
        {
            var main = game.mains[this.id];
            game.PlayedDominos.AddRange(domino.Values);
            main.Remove(domino);
            return new Action { name = "domino", domino = domino };

        }
    }
}

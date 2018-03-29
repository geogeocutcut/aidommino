using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public abstract class Player
    {

        public int nbPioche;

        public string id=Guid.NewGuid().ToString();
        public string name;
        public int generation;

        public int nbDominoInitial;
        public int nbPiocheInitial;

        public abstract void Initialize(GameIA game);

        public abstract void UpdateState(GameIA game,Player enemy, Action action);

        public abstract Action NextAction(GameIA game);
        public abstract Action StartGame(GameIA game,Domino domino);
    }
}

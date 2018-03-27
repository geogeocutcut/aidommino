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
        public GameIA game;
        public List<Domino> Main = new List<Domino>();

        public int nbDominoInitial;
        public int nbPiocheInitial;

        public abstract void Initialize(GameIA gameTmp);

        public abstract void UpdateState(Player enemy, Action action);

        public abstract Action NextAction();
        public abstract Action StartGame(Domino domino);
    }
}

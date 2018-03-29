using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public abstract class IAPlayer:Player
    {
        public double coeff_double { get; set; }
        public double coeff_div { get; set; }
        public double coeff_valeur { get; set; }
        public double coeff_bloq { get; set; }
        public double coeff_incertitude { get; set; }
        public double indice_mutuabilite { get; set; }


        public abstract override void Initialize(GameIA game);

        public abstract override void UpdateState(GameIA game,Player enemy, Action action);

        public abstract override Action NextAction(GameIA game);
        public abstract override Action StartGame(GameIA game,Domino domino);
    }
}

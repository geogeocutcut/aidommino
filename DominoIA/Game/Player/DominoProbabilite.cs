using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class DominoProbabilite
    {
        public Domino domino { get; set; }

        public double proba { get; set; }

        public DominoProbabilite(Domino d , double prob)
        {
            domino = d;
            proba = prob;
        }
    }
}

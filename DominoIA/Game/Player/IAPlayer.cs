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
        public double coeff_played { get; set; }
        public double indice_mutuabilite { get; set; }

        
    }
}

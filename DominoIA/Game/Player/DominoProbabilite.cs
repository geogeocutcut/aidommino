namespace DominoIA.Game
{
    public class DominoProbabilite
    {
        public Domino domino { get; set; }

        public double proba { get; set; }

        public double coeff_pioche { get; set; }

        public DominoProbabilite PiocheProb { get; set; }

        public DominoProbabilite(Domino d , double prob)
        {
            domino = d;
            proba = prob;
            coeff_pioche = 0;
        }

        public double valeur
        {
            get
            {
                return PiocheProb!=null?proba + coeff_pioche * PiocheProb.proba: proba;
            }
        }
    }
}

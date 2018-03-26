

using System.Linq;

namespace DominoIA.Game
{
    public class Domino
    {
        public int[] Values = new int[2];

        public double[] scores = new double[2];

        public int GetValue()
        {
            return Values.Sum();
        }

        public bool IsDouble()
        {
            return Values[0]==Values[1];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class Population
    {
        Random rnd = new Random();
        public Player[] players = new Player[100];
        public Player[] loosers = new Player[10];
        public int[] played = new int[100];
        public int[] scores = new int[100];
        public int[] looserScores = new int[10];
        public int[] looserPlayed = new int[10];

        public void Initialize()
        {
            for(int i=0; i<players.Count();i++)
            {
                players[i] = new IAMediumPlayer
                {
                    name = "player " + i,
                    coeff_double = rnd.NextDouble() * 10,
                    coeff_valeur = rnd.NextDouble() * 10,
                    coeff_div = rnd.NextDouble() * 10,
                    coeff_bloq = rnd.NextDouble() * 10,
                    indice_mutuabilite = (rnd.NextDouble() - 0.5)
                };
                //players[i] = new DummyPlayer
                //{
                //    name = "player " + i
                //};
            }

            for (int i = 0; i < loosers.Count(); i++)
            {
                loosers[i] = new IADummyPlayer
                {
                    name = "looser"
                };
            }
        }

        public void Reproduction()
        {
            int topWinners = 10;
            var classement = players.Zip(scores.Zip(played,(s,p)=>p>0?(double)s/(double)p:0), (p, s) => new { pl = p, sc = s }).OrderByDescending(cp => cp.sc);

            for (int i = 0; i < topWinners; i++)
            {
                if(classement.ElementAt(i).pl is IAPlayer)
                {
                    var winner = (IAPlayer) classement.ElementAt(i).pl;
                    var player = (IAPlayer) classement.ElementAt(i + (100 - topWinners)).pl;
                    var mutabilite = winner.indice_mutuabilite;
                    player.coeff_valeur = mutation(winner.coeff_valeur, mutabilite);
                    player.coeff_double = mutation(winner.coeff_double, mutabilite);
                    player.coeff_div = mutation(winner.coeff_div, mutabilite);
                    player.coeff_bloq = mutation(winner.coeff_bloq, mutabilite);
                    player.coeff_incertitude = mutation(winner.coeff_incertitude, mutabilite);
                    player.indice_mutuabilite = mutation(winner.indice_mutuabilite, mutabilite);
                    player.generation += 1;
                }
            }

            scores = new int[100];
            played = new int[100];
            looserScores = new int[10];
            looserPlayed = new int[10];
        }

        public double mutation(double coeff, double mutabilite)
        {
            return coeff * (1 + mutabilite * (2 * rnd.NextDouble() - 1));
        }
    }
}

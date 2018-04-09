using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class ClassementItem
    {
        public Player pl;
        public double score
        {
            get { return played > 0 ? point / played : 0; }
        }
        public double point;
        public double played;
        public int classement;
    }
    public class Population
    {
        Random rnd = new Random();
        public Player[] players = new Player[100];
        public Player[] loosers = new Player[10];
        public Dictionary<Player, ClassementItem> classement = new Dictionary<Player, ClassementItem>();

        public IEnumerable<ClassementItem>  Classement
        {
            get { return classement.OrderByDescending(x => x.Value.score).Select(x => x.Value); }
        }

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
                    coeff_incertitude = rnd.NextDouble() * 10,
                    indice_mutuabilite = (rnd.NextDouble() - 0.5)
                };
            }

            for (int i = 0; i < loosers.Count(); i++)
            {
                loosers[i] = new IADummyPlayer
                {
                    name = "looser"
                };
            }
            InitializeClassement();
        }
        

        public void Reproduction()
        {
            var winners = Classement.Take(10).Select(x=>x.pl);
            var loosers = Classement.Skip(90).Select(x => x.pl);

            var iterations = Math.Min(winners.Count(), loosers.Count());   


            for (int i = 0; i < iterations; i++)
            {
                var indloos = loosers.Count() - 1 - i;
                if (indloos>0 && loosers.ElementAt(indloos) is IAPlayer)
                {
                    var winner = (IAPlayer)winners.ElementAt(i);
                    var player = (IAPlayer)loosers.ElementAt(indloos);
                    var mutabilite = winner.indice_mutuabilite;
                    player.coeff_valeur = mutation(winner.coeff_valeur, mutabilite);
                    player.coeff_double = mutation(winner.coeff_double, mutabilite);
                    player.coeff_div = mutation(winner.coeff_div, mutabilite);
                    player.coeff_bloq = mutation(winner.coeff_bloq, mutabilite);
                    player.coeff_incertitude = mutation(winner.coeff_incertitude, mutabilite);
                    player.indice_mutuabilite = mutation(winner.indice_mutuabilite, mutabilite);
                    player.generation = winner.generation + 1;
                    player.name = winner.name;
                }
            }
            InitializeClassement() ;
        }

        private void InitializeClassement()
        {
            classement = new Dictionary<Player, ClassementItem>();
            foreach (var p in players)
            {
                classement[p] = new ClassementItem { pl = p };
            }
        }

        public double mutation(double coeff, double mutabilite)
        {
            return coeff * (1 + mutabilite * (2 * rnd.NextDouble() - 1));
        }
    }
}

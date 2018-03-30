using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class IAMediumPlayer : IAPlayer
    {
        

        public IAMediumPlayer()
        {
        }
        public IAMediumPlayer(IAMediumPlayer pl)
        {
            name = pl.name;
            coeff_double = pl.coeff_double;
            coeff_div = pl.coeff_div;
            coeff_valeur = pl.coeff_valeur;
            coeff_bloq = pl.coeff_bloq;
            coeff_incertitude = pl.coeff_incertitude;
            indice_mutuabilite = pl.indice_mutuabilite;
        }
        public override void Initialize (GameIA game)
        {

            var main = game.mains[this.id];

            while (main.Count < game.nbDominoMainInitial)
            {
                var index = GameIA.rnd.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                main.Add(domino);
            }
        }
        
        public override void UpdateState(GameIA game,Player enemy,Action action)
        {
            switch(action.name)
            {
                case "domino":
                    break;
                case "pioche":
                    break;
                case "passe":
                    break;
            }
        }

        public override Action NextAction(GameIA game)
        {
            var main = game.mains[this.id];
            // Evaluation des dominos dans la main
            var leftNum = game.PlayedDominos.First();
            var rightNum = game.PlayedDominos.Last();
            var possibleDominos = main.Where(d => d.Values.Any(i => i== leftNum || i == rightNum));

            // Recupération
            Domino playDomino;
            if (possibleDominos.Any())
            {
                // scoring IA
                CalculScore(main,possibleDominos, leftNum,rightNum);
                playDomino = possibleDominos.OrderByDescending(d => d.scores.Max()).First();

                main.Remove(playDomino);


                if (playDomino.scores[0] > playDomino.scores[1])
                {
                    if (playDomino.Values[0] == leftNum)
                    {
                        game.PlayedDominos.Insert(0, playDomino.Values[0]);
                        game.PlayedDominos.Insert(0, playDomino.Values[1]);
                    }
                    else
                    {
                        game.PlayedDominos.Add(playDomino.Values[0]);
                        game.PlayedDominos.Add(playDomino.Values[1]);
                    }
                }
                else
                {
                    if (playDomino.Values[1] == leftNum)
                    {
                        game.PlayedDominos.Insert(0, playDomino.Values[1]);
                        game.PlayedDominos.Insert(0, playDomino.Values[0]);
                    }
                    else
                    {
                        game.PlayedDominos.Add(playDomino.Values[1]);
                        game.PlayedDominos.Add(playDomino.Values[0]);
                    }
                }
                return new Action { name = "domino", domino = playDomino };
            }
            if(game.Pioche.Any())
            {
                var index = GameIA.rnd.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                main.Add(domino);
                return new Action { name = "pioche" };
            }

            return new Action { name = "passe" };
        }
        public void CalculScore(IEnumerable<Domino> main,IEnumerable<Domino> possibleDominos,int leftNum,int rightNum)
        {
            var possibleVal = new[] { leftNum, rightNum };
            foreach(Domino d in possibleDominos)
            {
                var scoreDouble = d.IsDouble() ? 1 : 0;
                for(int i =0;i<2;i++)
                {
                    var val = d.Values[1 - i];
                    if(possibleVal.Contains(d.Values[i]))
                    {
                        d.scores[i] = coeff_double * scoreDouble + coeff_valeur * d.GetValue() + coeff_div * GetDiversiteMain(main,d, val);
                    }
                    else
                    {
                        d.scores[i] = 0;
                    }
                }
            }
        }
        

        public double GetDiversiteMain(IEnumerable<Domino> main,Domino d, int val)
        {
            var result = main.Count(t => t != d && t.Values.Contains(val));
            return result;
        }

        public override Action StartGame(GameIA game,Domino domino)
        {
            var main = game.mains[this.id];
            game.PlayedDominos.AddRange(domino.Values);
            main.Remove(domino);
            return new Action { name = "domino", domino = domino };

        }
    }
}

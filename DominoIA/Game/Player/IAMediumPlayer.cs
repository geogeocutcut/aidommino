using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class IAMediumPlayer : Player
    {

        public double coeff_double { get; set; }
        public double coeff_div { get; set; }
        public double coeff_valeur { get; set; }
        public double coeff_bloq { get; set; }
        public double coeff_incertitude { get; set; }
        public double indice_mutuabilite { get; set; }
        

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
        public override void Initialize (GameIA gameTmp)
        {
            game = gameTmp;
            

            while (Main.Count<6)
            {
                var index = GameIA.rnd.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                Main.Add(domino);
            }
        }
        
        public override void UpdateState(Player enemy,Action action)
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

        public override Action NextAction()
        {
            // Evaluation des dominos dans la main
            var leftNum = game.PlayedDominos.First();
            var rightNum = game.PlayedDominos.Last();
            var possibleDominos = Main.Where(d => d.Values.Any(i => i== leftNum || i == rightNum));

            // score = coeff_double*score_double + coeff_div*score_div  + coeff_valeur * score_valeur + coeff_bloq * score_bloq
            // Recupération
            Domino playDomino;
            if (possibleDominos.Any())
            {
                // scoring IA
                CalculScore(possibleDominos, leftNum,rightNum);
                playDomino = possibleDominos.OrderByDescending(d => d.scores.Max()).First();

                Main.Remove(playDomino);


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
                Main.Add(domino);
                return new Action { name = "pioche" };
            }

            return new Action { name = "passe" };
        }
        public void CalculScore(IEnumerable<Domino> possibleDominos,int leftNum,int rightNum)
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
                        d.scores[i] = coeff_double * scoreDouble + coeff_valeur * d.GetValue() + coeff_div * GetDiversiteMain(d,val);
                    }
                    else
                    {
                        d.scores[i] = 0;
                    }
                }
            }
        }
        

        public double GetDiversiteMain(Domino d, int val)
        {
            var result = Main.Count(t => t != d && t.Values.Contains(val));
            return result;
        }

        public override Action StartGame(Domino domino)
        {
            game.PlayedDominos.AddRange(domino.Values);
            Main.Remove(domino);
            return new Action { name = "domino", domino = domino };

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class IAHardPlayer : Player
    {

        public double coeff_double { get; set; }
        public double coeff_div { get; set; }
        public double coeff_valeur { get; set; }
        public double coeff_bloq { get; set; }
        public double coeff_incertitude { get; set; }
        public double indice_mutuabilite { get; set; }

        public List<Action> actions = new List<Action>();
        public List<DominoProbabilite> PiocheProbalibites = new List<DominoProbabilite>();
        public Dictionary<Player,List<DominoProbabilite>> EnemyPossibleMains = new Dictionary<Player, List<DominoProbabilite>>();

        public IAHardPlayer()
        {
        }
        public IAHardPlayer(IAHardPlayer pl)
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
            var possibleDominos = game.Dominos.Where(d => !Main.Contains(d));
            foreach (var e in game.players)
            {
                if (e != this)
                {
                    EnemyPossibleMains[e] = new List<DominoProbabilite>();
                    foreach (var d in possibleDominos)
                    {
                        EnemyPossibleMains[e].Add(new DominoProbabilite(d, 6.0 / possibleDominos.Count()));
                    }
                }
            }
            foreach (var d in possibleDominos)
            {
                PiocheProbalibites.Add(new DominoProbabilite(d, 4.0 / possibleDominos.Count()));
            }
        }
        
        public override void UpdateState(Player enemy,Action action)
        {
            
            var possibleNum = new[] { game.PlayedDominos.First(), game.PlayedDominos.Last() };
            var nbDominoNonJoue = game.Dominos.Where(d=>!Main.Contains(d)).Count();
            var possibleDominos = game.Dominos.Where(d => !Main.Contains(d)).Where(d => possibleNum.Contains(d.Values[0]) || possibleNum.Contains(d.Values[1]));
            var nbPossibleDominos = possibleDominos.Count();
            var nbDominoMainEnemy = enemy.Main.Count();

            switch (action.name)
            {
                case "domino":
                    var proba = EnemyPossibleMains[enemy].First(p => p.domino == action.domino).proba;
                    foreach (var prob in EnemyPossibleMains[enemy])
                    {
                        if (prob.domino == action.domino)
                        {
                            prob.proba = 0;

                            // Mise à jour des proba des autres joueurs 
                            foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy).SelectMany(p => p.Value).Where(p => p.domino == prob.domino))
                            {
                                prob2.proba = 0;
                            }

                            //Mise à jour des proba de la pioche
                            var prob3 = PiocheProbalibites.First(p => p.domino == prob.domino);
                            prob3.proba = 0;
                        }
                        else
                        {
                            var proba2 = prob.proba;
                            prob.proba = prob.proba * (double)nbDominoMainEnemy/(double)(nbDominoMainEnemy+1 - proba);

                            // Mise à jour des proba des autres joueurs 
                            foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy).SelectMany(p => p.Value).Where(p => p.domino == prob.domino))
                            {
                                prob2.proba = prob2.proba * (double)(1 - prob.proba) / (double)(1 - proba2);
                            }

                            //Mise à jour des proba de la pioche
                            var prob3 = PiocheProbalibites.First(p => p.domino == prob.domino);
                            prob3.proba = prob3.proba * (double)(1 - prob.proba) / (double)(1 - proba2);
                        }
                    }
                    break;
                case "pioche":
                    // mise àjour proba pour les dominos non présent dans la main du player
                    foreach (var prob in EnemyPossibleMains[enemy].Where(p => possibleDominos.Contains(p.domino)))
                    {
                        foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy)
                            .SelectMany(p => p.Value)
                            .Where(p => p.domino==prob.domino))
                        {
                            prob2.proba = prob2.proba * 1 / (1 - prob.proba);
                        }

                        foreach (var prob2 in PiocheProbalibites.Where(p => p.domino==prob.domino))
                        {
                            prob2.proba = prob2.proba * 1 / (1 - prob.proba);
                        }
                        prob.proba = 0;
                    }

                    // mise à jour des prob pour les dominos présent dans la main du player
                    foreach (var prob in EnemyPossibleMains[enemy].Where(p => !possibleDominos.Contains(p.domino)))
                    {
                        var probatmp = prob.proba * 1 / (1 - (double)nbPossibleDominos / (double)nbDominoNonJoue);
                        foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy)
                            .SelectMany(p => p.Value)
                            .Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * (double)(1-probatmp) / (double)(1 - prob.proba);
                        }

                        foreach (var prob2 in PiocheProbalibites.Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * (double)(1 - probatmp) / (double)(1 - prob.proba);
                        }
                        prob.proba = probatmp;
                    }

                    // Mise à jour suite à la pioche
                    foreach (var prob in EnemyPossibleMains[enemy])
                    {
                        var piocheProb = PiocheProbalibites.First(p => p.domino == prob.domino);
                        var piocheProba = piocheProb.proba / (game.Pioche.Count + 1);
                        piocheProb.proba = piocheProba * game.Pioche.Count;
                        prob.proba += piocheProba;
                    }
                    
                    break;
                case "passe":
                    // mise àjour proba pour les dominos non présent dans la main du player
                    foreach (var prob in EnemyPossibleMains[enemy].Where(p => possibleDominos.Contains(p.domino)))
                    {
                        foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy)
                            .SelectMany(p => p.Value)
                            .Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * 1 / (1 - prob.proba);
                        }

                        foreach (var prob2 in PiocheProbalibites.Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * 1 / (1 - prob.proba);
                        }
                        prob.proba = 0;
                    }

                    // mise à jour des prob pour les dominos présent dans la main du player
                    foreach (var prob in EnemyPossibleMains[enemy].Where(p => !possibleDominos.Contains(p.domino)))
                    {
                        var probatmp = prob.proba * 1 / (1 - (double)nbPossibleDominos / (double)nbDominoNonJoue);
                        foreach (var prob2 in EnemyPossibleMains.Where(k => k.Key != enemy)
                            .SelectMany(p => p.Value)
                            .Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * (double)(1 - probatmp) / (double)(1 - prob.proba);
                        }

                        foreach (var prob2 in PiocheProbalibites.Where(p => p.domino == prob.domino))
                        {
                            prob2.proba = prob2.proba * (double)(1 - probatmp) / (double)(1 - prob.proba);
                        }
                        prob.proba = probatmp;
                    }
                    break;
            }

            var totalProbbyDomino = EnemyPossibleMains.SelectMany(p => p.Value).Concat(PiocheProbalibites).GroupBy(p=>p.domino).Select(gr=>new { domino = gr.Key, s = gr.Sum(p => p.proba) }).Where(p=>game.Dominos.Contains(p.domino));
            foreach (var d in totalProbbyDomino.Where(p=>p.s<0.95||p.s>1.05))
            {
                Console.WriteLine("erreur proba domino : " + action.name+" : "+ d.domino+" ; "+d.s);
            }

            var totalProbbyPlayer = EnemyPossibleMains.Select(gr => new { player = gr.Key, s = gr.Value.Sum(p => p.proba)/gr.Key.Main.Count });
            foreach (var d in totalProbbyPlayer.Where(p => p.s < 0.95 || p.s > 1.05))
            {
                Console.WriteLine("erreur proba player : " + action.name + " : " + d.player.id + " ; " + d.s);
            }

            var scorePioche = game.Pioche.Count>0?PiocheProbalibites.Sum(p => p.proba) / game.Pioche.Count:1;
        }

        public override Action NextAction()
        {
            // Evaluation des dominos dans la main
            var leftNum = game.PlayedDominos.First();
            var rightNum = game.PlayedDominos.Last();
            var possibleDominos = Main.Where(d => d.Values.Any(i => i== leftNum || i == rightNum));

            // score = coeff_double*score_double + coeff_div*score_div  + coeff_valeur * score_valeur + coeff_bloq * score_bloq
            // Recupération
            Action action;
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
                action = new Action { name = "domino", domino = playDomino };
                actions.Add(action);
                return action;
            }
            if(game.Pioche.Any())
            {
                var index = GameIA.rnd.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                Main.Add(domino);
                foreach(var prob in EnemyPossibleMains)
                {
                    prob.Value.RemoveAll(p => p.domino == domino);
                }
                action = new Action { name = "pioche" };

                actions.Add(action);
                return action;
            }

            action = new Action { name = "passe" };
            actions.Add(action);
            return action;
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
                        d.scores[i] = coeff_double * scoreDouble + coeff_valeur * d.GetValue() + coeff_div * GetDiversiteMain(d,val) + coeff_bloq * GetScoreBlocage(d,val, possibleVal);
                    }
                    else
                    {
                        d.scores[i] = 0;
                    }
                }
            }
        }

        private double GetScoreBlocage(Domino d, int val, int[] possibleVal)
        {
            var dval = d.Values.Count(dv => dv != val) >0 ? d.Values.First(dv => dv != val) : val;
            var dval2 = possibleVal.Count(dv=>dv!=dval)>0? possibleVal.First(dv => dv != dval):val;
            var result = EnemyPossibleMains.SelectMany((k) => k.Value).Count(p => p.domino.Values.Contains(dval)|| p.domino.Values.Contains(dval));
            return result == 0 ? 0 : 1; 
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

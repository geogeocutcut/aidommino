using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoIA.Game
{
    public class IAHardPlayer : IAPlayer
    {

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

        public override void FinalizeInitialisation(GameRunIA game)
        {
            var main = game.mains[this.id];

            game.playerProbabilites[this] = new Dictionary<Player, Dictionary<Domino, DominoProbabilite>>();
            game.dominoProbabilites[this] = new Dictionary<Domino, Dictionary<Player, DominoProbabilite>>();
            game.piocheProbabilites[this] = new Dictionary<Domino, DominoProbabilite>();

            var enemyPossibleMains = game.playerProbabilites[this];
            var dominoPossibleMains = game.dominoProbabilites[this];

            var possibleDominos = game.Dominos.Where(d => !main.Contains(d));
            foreach (var d in possibleDominos)
            {
                dominoPossibleMains[d] = new Dictionary < Player, DominoProbabilite > ();
            }


            foreach (var e in game.players.Where(p=>p.Key!=this.id))
            {
                enemyPossibleMains[e.Value] = new Dictionary<Domino, DominoProbabilite>();
                foreach (var d in possibleDominos)
                {
                    var prob = new DominoProbabilite(d, 1);
                    enemyPossibleMains[e.Value][d]= prob;
                    dominoPossibleMains[d][e.Value] = prob;
                }
            }

            var piocheProbalibites = game.piocheProbabilites[this];
            foreach (var d in possibleDominos)
            {
                piocheProbalibites[d]=new DominoProbabilite(d, 1);
            }
        }

        public override void UpdateState(GameRunIA game,Player enemy, Action action)
        {
            var coeffPioche = (double) 1/ (double)(game.nbDominoPiocheInitial + 1);
            var dominoMainPossible = game.dominoProbabilites[this];
            var enemyMainPossible = game.playerProbabilites[this];
            var piochePossible = game.piocheProbabilites[this];

            var main = game.mains[this.id];
            var possibleNum = new[] { game.PlayedDominos.First(), game.PlayedDominos.Last() };

            var possibleDominos = game.Dominos.Where(d => !main.Contains(d)).Where(d => possibleNum.Contains(d.Values[0]) || possibleNum.Contains(d.Values[1]));

            switch (action.name)
            {
                case "domino":
                    foreach (var d in dominoMainPossible[action.domino])
                    {
                        if(d.Key==enemy)
                        {
                            if(d.Value.proba<1)
                            {
                                foreach(var d2 in enemyMainPossible[enemy].Where(x=>x.Value.proba<1))
                                {
                                    d2.Value.proba -= coeffPioche;
                                }
                            }
                        }
                        d.Value.proba = 0;
                    }
                    piochePossible[action.domino].proba = 0;
                    UpdateProbabilite(game, dominoMainPossible, enemyMainPossible, piochePossible);
                    break;
                case "pioche":
                    foreach (var d in possibleDominos)
                    {
                        enemyMainPossible[enemy][d].proba = enemyMainPossible[enemy][d].proba<1? enemyMainPossible[enemy][d].proba+ coeffPioche : coeffPioche;
                    }
                    foreach (var d in piochePossible.Where(x=>x.Value.proba>0))
                    {
                        if(enemyMainPossible[enemy][d.Key].proba<1)
                        {
                            enemyMainPossible[enemy][d.Key].proba += coeffPioche;
                        }
                    }
                    break;
                case "passe":
                    foreach (var d in possibleDominos)
                    {
                        enemyMainPossible[enemy][d].proba = 0;
                    }
                    UpdateProbabilite(game, dominoMainPossible, enemyMainPossible, piochePossible);
                    break;
            }
        }

        private void UpdateProbabilite(GameRunIA game, Dictionary<Domino, Dictionary<Player, DominoProbabilite>> dominoMainPossible, Dictionary<Player, Dictionary<Domino, DominoProbabilite>> enemyMainPossible, Dictionary<Domino, DominoProbabilite> piochePossible)
        {
            bool updateProb = true;
            while (updateProb)
            {
                updateProb = false;
                foreach (var p in enemyMainPossible)
                {
                    if (p.Value.Sum(p2 => p2.Value.proba) <= game.mains[p.Key.id].Count)
                    {
                        foreach (var prob in p.Value)
                        {
                            if (prob.Value.proba > 0)
                            {
                                foreach (var prob2 in dominoMainPossible[prob.Key].Where(x => x.Key != p.Key))
                                {
                                    if (prob2.Value.proba > 0)
                                    {
                                        prob2.Value.proba = 0;
                                        updateProb = true;
                                    }
                                }
                                if (piochePossible[prob.Key].proba > 0)
                                {
                                    piochePossible[prob.Key].proba = 0;
                                    updateProb = true;
                                }
                            }
                        }
                    }
                }
                if (piochePossible.Sum(p2 => p2.Value.proba) <= game.Pioche.Count)
                {
                    foreach (var prob in piochePossible)
                    {
                        if (prob.Value.proba > 0)
                        {
                            foreach (var prob2 in dominoMainPossible[prob.Key])
                            {
                                if (prob2.Value.proba > 0)
                                {
                                    prob2.Value.proba = 0;
                                    updateProb = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override Action NextAction(GameRunIA game)
        {
            var main = game.mains[this.id];

            // Evaluation des dominos dans la main
            var leftNum = game.PlayedDominos.First();
            var rightNum = game.PlayedDominos.Last();
            var possibleDominos = main.Where(d => d.Values.Any(i => i == leftNum || i == rightNum));
            
            // Recupération 
            Action action;
            Domino playDomino;
            if (possibleDominos.Any())
            {
                // scoring IA
                CalculScore(game,possibleDominos, leftNum, rightNum);
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
                action = new Action { name = "domino", domino = playDomino };
                return action;
            }
            if (game.Pioche.Any())
            {
                var index = StaticRandom.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                main.Add(domino);
                UpdateProbabilite(game,domino);
                action = new Action {domino=domino, name = "pioche" };

                return action;
            }

            action = new Action { name = "passe" };
            return action;
        }

        private void UpdateProbabilite(GameRunIA game,Domino domino)
        {
            var dominoMainPossible = game.dominoProbabilites[this];
            var enemyMainPossible = game.playerProbabilites[this];
            var piochePossible = game.piocheProbabilites[this];
            foreach (var p in dominoMainPossible[domino].Values)
            {
                p.proba = 0;
            }
            piochePossible[domino].proba = 0;
            UpdateProbabilite(game, dominoMainPossible, enemyMainPossible, piochePossible);
        }

        public void CalculScore(GameRunIA game, IEnumerable<Domino> possibleDominos, int leftNum, int rightNum)
        {
            var main = game.mains[this.id];
            var possibleVal = new[] { leftNum, rightNum };
            foreach (Domino d in possibleDominos)
            {
                var scoreDouble = d.IsDouble() ? 1 : 0;
                for (int i = 0; i < 2; i++)
                {
                    var val = d.Values[1 - i];
                    if (possibleVal.Contains(d.Values[i]))
                    {
                        var scoreValue = d.GetValue();
                        var scoreDivers = GetDiversiteMain(main, d, val);
                        var scoreBlocage = GetScoreBlocage(game, d, val, possibleVal);
                        d.scores[i] = coeff_double * scoreDouble + coeff_valeur * scoreValue + coeff_div * scoreDivers + scoreBlocage;
                    }
                    else
                    {
                        d.scores[i] = 0;
                    }
                }
            }
        }
        
        private double GetScoreBlocage(GameRunIA game,Domino d, int val, int[] possibleVal)
        {
            var dval = val;
            var dval2 = possibleVal.Count(dv => dv != dval) > 0 ? possibleVal.First(dv => dv != dval) : val;
            var enemiesPossibleMain = game.playerProbabilites[this]
                .Select(x => new {
                    pl = x.Key,
                    nbDominoBloqueMain = x.Value.Where(v => !v.Key.Values.Contains(dval) && !v.Key.Values.Contains(dval2)).Count(v=>v.Value.proba>0),
                    nbDominoTotalMain = x.Value.Count(v => v.Value.proba>0) });
            var nbDominoBloques = enemiesPossibleMain.Sum(p => p.nbDominoBloqueMain);
            var nbDominoPossibles = enemiesPossibleMain.Sum(p => p.nbDominoTotalMain);
            var blocage = coeff_bloq * enemiesPossibleMain.Where(x => x.nbDominoBloqueMain >= game.mains[x.pl.id].Count).Count() ;
            var blocage_incert = coeff_incertitude *nbDominoBloques / nbDominoPossibles;
            return blocage+ blocage_incert;
        }

        public double GetDiversiteMain(IEnumerable<Domino> main,Domino d, int val)
        {
            var result = main.Count(t => t != d && t.Values.Contains(val));
            return result;
        }

        public override void PrintDescription()
        {
            Console.WriteLine("Player hard : " + this.name + " / " + this.id);
            Console.WriteLine("    generation : " + this.generation);
            Console.WriteLine("    coeff double : " + this.coeff_double);
            Console.WriteLine("    coeff diversité : " + this.coeff_div);
            Console.WriteLine("    coeff valeur : " + this.coeff_valeur);
            Console.WriteLine("    coeff bloquage : " + this.coeff_bloq);
            Console.WriteLine("    coeff incertitude : " + this.coeff_incertitude);
        }
    }
}

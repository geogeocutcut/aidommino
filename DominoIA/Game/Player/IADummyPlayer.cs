﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoIA.Game
{
    public class IADummyPlayer:IAPlayer
    {
        
        public override void Initialize (GameIA game)
        {
            var main = game.mains[this.id];
            while (main.Count< game.nbDominoMainInitial)
            {
                var index = GameIA.rnd.Next(game.Pioche.Count);
                var domino = game.Pioche[index];
                game.Pioche.RemoveAt(index);
                main.Add(domino);
            }
        }
        
        

        public override Action NextAction(GameIA game)
        {
            // Evaluation des dominos dans la main
            var leftNum = game.PlayedDominos.First();
            var rightNum = game.PlayedDominos.Last();
            var main = game.mains[this.id];
            var possibleLeftDominos = main.Where(d => d.Values.Any(i => i== leftNum));
            var possibleRightDominos = main.Where(d => d.Values.Any(i => i == rightNum));

            // score = coeff_double*score_double + coeff_div*score_div  + coeff_valeur * score_valeur + coeff_bloq * score_bloq
            // Recupération
            Domino playDomino;
            if (possibleLeftDominos.Any())
            {
                // basic IA
                playDomino = possibleLeftDominos.First();

                main.Remove(playDomino);
                if(playDomino.Values[0]==leftNum)
                {
                    game.PlayedDominos.Insert(0, playDomino.Values[0]);
                    game.PlayedDominos.Insert(0, playDomino.Values[1]);
                }
                else
                {
                    game.PlayedDominos.Insert(0, playDomino.Values[1]);
                    game.PlayedDominos.Insert(0, playDomino.Values[0]);
                }

                return new Action { name = "domino", domino = playDomino };
            }
            if (possibleRightDominos.Any())
            {
                // basic IA
                playDomino = possibleRightDominos.First();

                main.Remove(playDomino);
                if (playDomino.Values[0] == rightNum)
                {
                    game.PlayedDominos.Add(playDomino.Values[0]);
                    game.PlayedDominos.Add(playDomino.Values[1]);
                }
                else
                {
                    game.PlayedDominos.Add(playDomino.Values[1]);
                    game.PlayedDominos.Add(playDomino.Values[0]);
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
        public override Action StartGame(GameIA game,Domino domino)
        {
            var main = game.mains[this.id];
            game.PlayedDominos.AddRange(domino.Values);
            main.Remove(domino);
            return new Action { name = "domino", domino = domino };

        }

        public override void UpdateState(GameIA game, Player enemy, Action action)
        {
        }
    }
}

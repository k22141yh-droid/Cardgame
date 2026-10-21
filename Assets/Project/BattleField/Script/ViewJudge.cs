using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.MasterData;

namespace BattleField.Script.Judge {
    public enum JudgementType {
        Win = 0,Draw = 1,Lose = 2
    }
    public class ViewJudge
    {
        public JudgementType JankenJudge(CardHand playerHand, CardHand enemyHand) {
            if (playerHand == CardHand.Yoshimoto) return JudgementType.Win;
            int _result = (HandExchanger(playerHand) - HandExchanger(enemyHand) + 3) % 3;
            if (_result == 0) return JudgementType.Draw;
            if (_result == 1) return JudgementType.Lose;
            return JudgementType.Win;
        }
        private int HandExchanger(CardHand hand) {
            if (hand == CardHand.Rock) return 0;//グーなら0
            if (hand == CardHand.Scissors) return 1;//チョキなら1
            return 2;//パーなら2が返る
        }
    }
}


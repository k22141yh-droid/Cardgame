using System;
using System.Collections.Generic;
using UnityEngine;

using Common.MasterData;

namespace BattleField.Script.Damage {
    class DamageValue
    {
        public int CardTypeExchanger(CardType type) {
            if (type == CardType.Fire) return 0;
            if (type == CardType.Grass) return 1;
            return 2;
        }

        // 相性: 1 = ばつぐん(1.25 倍) / 0 = ふつう / -1 = いまひとつ(0.75 倍)
        public int Matchup(CardType attacker, CardType receiver) {
            if (attacker == CardType.Yoshimoto) return 0;
            int _result = (CardTypeExchanger(attacker) - CardTypeExchanger(receiver) + 3) % 3;
            if (_result == 0) return 0; // タイプが同じ
            if (_result == 1) return -1;
            return 1;
        }

        public int CalcDamageValue(int power, CardType attacker, CardType receiver) {
            int matchup = Matchup(attacker, receiver);
            if (matchup < 0) return (int)Math.Round(power * 0.75);
            if (matchup > 0) return (int)Math.Round(power * 1.25);
            return power;
        }

        public int DamageBalance(int HP, int Damage) {
            if(HP < Damage) return HP;
            return Damage;
        }

    }
}

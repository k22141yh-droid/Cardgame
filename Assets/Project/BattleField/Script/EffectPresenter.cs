using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.MasterData;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using BattleField.Script.TimelineManagge;

namespace BattleField.Script.Effect
{
    public class EffectPresenter : MonoBehaviour
    {
        // --- Level 3 ---
        [SerializeField] TimelineManagger _level3FireRockEffect;
        [SerializeField] TimelineManagger _level3FireScissorsEffect;
        [SerializeField] TimelineManagger _level3FirePaperEffect;
        [SerializeField] TimelineManagger _level3WaterRockEffect;
        [SerializeField] TimelineManagger _level3WaterScissorsEffect;
        [SerializeField] TimelineManagger _level3WaterPaperEffect;
        [SerializeField] TimelineManagger _level3GrassRockEffect;
        [SerializeField] TimelineManagger _level3GrassScissorsEffect;
        [SerializeField] TimelineManagger _level3GrassPaperEffect;

        // --- Level 2 ---
        [SerializeField] TimelineManagger _level2FireRockEffect;
        [SerializeField] TimelineManagger _level2FireScissorsEffect;
        [SerializeField] TimelineManagger _level2FirePaperEffect;
        [SerializeField] TimelineManagger _level2WaterRockEffect;
        [SerializeField] TimelineManagger _level2WaterScissorsEffect;
        [SerializeField] TimelineManagger _level2WaterPaperEffect;
        [SerializeField] TimelineManagger _level2GrassRockEffect;
        [SerializeField] TimelineManagger _level2GrassScissorsEffect;
        [SerializeField] TimelineManagger _level2GrassPaperEffect;

        // --- Level 1 ---
        [SerializeField] TimelineManagger _level1FireRockEffect;
        [SerializeField] TimelineManagger _level1FireScissorsEffect;
        [SerializeField] TimelineManagger _level1FirePaperEffect;
        [SerializeField] TimelineManagger _level1WaterRockEffect;
        [SerializeField] TimelineManagger _level1WaterScissorsEffect;
        [SerializeField] TimelineManagger _level1WaterPaperEffect;
        [SerializeField] TimelineManagger _level1GrassRockEffect;
        [SerializeField] TimelineManagger _level1GrassScissorsEffect;
        [SerializeField] TimelineManagger _level1GrassPaperEffect;

        // 威力 → エフェクトレベル
        private const int PowerLevel3 = 2000;
        private const int PowerLevel2 = 1000;
        private const int PowerLevel1 = 500;

        private const int HandCount = 3;

        private static int PowerToLevel(int power) => power switch
        {
            PowerLevel3 => 3,
            PowerLevel2 => 2,
            PowerLevel1 => 1,
            _ => 0
        };

        // 添字: タイプ(Fire, Water, Grass) * HandCount + 手(Rock, Scissors, Paper)
        private TimelineManagger[] GetLevelEffects(int level) => level switch
        {
            3 => new[] {
                _level3FireRockEffect, _level3FireScissorsEffect, _level3FirePaperEffect,
                _level3WaterRockEffect, _level3WaterScissorsEffect, _level3WaterPaperEffect,
                _level3GrassRockEffect, _level3GrassScissorsEffect, _level3GrassPaperEffect },
            2 => new[] {
                _level2FireRockEffect, _level2FireScissorsEffect, _level2FirePaperEffect,
                _level2WaterRockEffect, _level2WaterScissorsEffect, _level2WaterPaperEffect,
                _level2GrassRockEffect, _level2GrassScissorsEffect, _level2GrassPaperEffect },
            1 => new[] {
                _level1FireRockEffect, _level1FireScissorsEffect, _level1FirePaperEffect,
                _level1WaterRockEffect, _level1WaterScissorsEffect, _level1WaterPaperEffect,
                _level1GrassRockEffect, _level1GrassScissorsEffect, _level1GrassPaperEffect },
            _ => null
        };

        private TimelineManagger FindEffect(Card card, int level)
        {
            var effects = GetLevelEffects(level);
            if (effects == null) return null;

            int typeIndex = card.Type switch
            {
                CardType.Fire => 0,
                CardType.Water => 1,
                CardType.Grass => 2,
                _ => -1
            };
            int handIndex = card.Hand switch
            {
                CardHand.Rock => 0,
                CardHand.Scissors => 1,
                CardHand.Paper => 2,
                _ => -1
            };
            if (typeIndex < 0 || handIndex < 0) return null;

            return effects[typeIndex * HandCount + handIndex];
        }

        public async UniTask EffectProcess(Card card)
        {
            int level = PowerToLevel(card.Power);

            if (card.Type == CardType.Yoshimoto)
            {
                // 特殊カード処理（現行仕様を維持）
                _level3FireRockEffect?.TimelinePlay();
                _level3WaterRockEffect?.TimelinePlay();
                _level3GrassRockEffect?.TimelinePlay();
                await UniTask.WaitUntil(() => _level3GrassRockEffect == null || _level3GrassRockEffect.IsDone());
                return;
            }

            if (level == 0) return;

            TimelineManagger targetEffect = FindEffect(card, level);

            if (targetEffect != null)
            {
                targetEffect.TimelinePlay();
                await UniTask.WaitUntil(() => targetEffect.IsDone());
            }
        }
    }
}
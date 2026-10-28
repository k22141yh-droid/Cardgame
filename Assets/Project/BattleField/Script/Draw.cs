using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

using BattleField.Script.TimelineManagge;
using Common.MasterData;
using BattleField.Script.Effect;

namespace BattleField.Script.Battle.Draw.Judge {
    public class Draw : JudgeProcessBase
    {
        private const int DrawDamage = 1000;

        [SerializeField] TimelineManagger _draw;
        [SerializeField] private EffectPresenter _playerEffectPresenter;
        [SerializeField] private EffectPresenter _enemyEffectPresenter;
        public async UniTask DrawProcess(Card playerCard, Card enemyCard, CancellationToken ct) {
            _draw.TimelinePlay();
            SetJankenOptions(playerCard, enemyCard);
            await UniTask.WaitUntil(() => _draw.IsDone());
            await _playerEffectPresenter.EffectProcess(playerCard);
            await _enemyEffectPresenter.EffectProcess(enemyCard);
            // あいこは攻撃力や相性に関係なく、お互いに固定ダメージ
            int enemyReceiveDamage = ApplyFixedDamage(_enemyhp, DrawDamage);
            int playerReceiveDamage = ApplyFixedDamage(_playerhp, DrawDamage);
            _damageTextEffect.WinDamaged(enemyReceiveDamage);
            await PlayDamageTimeline();
            _damageTextEffect.LoseDamaged(playerReceiveDamage);
            await PlayDamageTimeline();
        }
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

using BattleField.Script.TimelineManagge;
using Common.MasterData;
using BattleField.Script.Effect;

namespace BattleField.Script.Battle.Lose.Judge {
    public class Lose : JudgeProcessBase
    {
        [SerializeField] TimelineManagger _lose;
        [SerializeField] private EffectPresenter _enemyEffectPresenter;
        public async UniTask LoseProcess(Card playerCard, Card enemyCard, CancellationToken ct) {
            _lose.TimelinePlay();
            SetJankenOptions(playerCard, enemyCard);
            await UniTask.WaitUntil(() => _lose.IsDone());
            await _enemyEffectPresenter.EffectProcess(enemyCard);
            int receiveDamage = ApplyDamage(_playerhp, enemyCard, playerCard);
            _damageTextEffect.LoseDamaged(receiveDamage);
            await PlayDamageTimeline();
        }
    }
}

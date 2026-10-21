using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

using BattleField.Script.TimelineManagge;
using Common.MasterData;
using BattleField.Script.Effect;

namespace BattleField.Script.Battle.Draw.Judge {
    public class Draw : JudgeProcessBase
    {
        [SerializeField] TimelineManagger _draw;
        [SerializeField] private EffectPresenter _playerEffectPresenter;
        [SerializeField] private EffectPresenter _enemyEffectPresenter;
        public async UniTask DrawProcess(Card playerCard, Card enemyCard, CancellationToken ct) {
            _draw.TimelinePlay();
            SetJankenOptions(playerCard, enemyCard);
            await UniTask.WaitUntil(() => _draw.IsDone());
            await _playerEffectPresenter.EffectProcess(playerCard);
            await _enemyEffectPresenter.EffectProcess(enemyCard);
            int enemyReceiveDamage = ApplyDamage(_enemyhp, playerCard, enemyCard);
            int playerReceiveDamage = ApplyDamage(_playerhp, enemyCard, playerCard);
            _damageTextEffect.WinDamaged(enemyReceiveDamage);
            await PlayDamageTimeline();
            _damageTextEffect.LoseDamaged(playerReceiveDamage);
            await PlayDamageTimeline();
        }
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

using BattleField.Script.TimelineManagge;
using Common.MasterData;
using BattleField.Script.Effect;

namespace BattleField.Script.Battle.Win.Judge {
    public class Win : JudgeProcessBase
    {
        [SerializeField] TimelineManagger _win;
        [SerializeField] private EffectPresenter _playerEffectPresenter;
        public async UniTask WinProcess(Card playerCard, Card enemyCard, CancellationToken ct) {
            _win.TimelinePlay();
            SetJankenOptions(playerCard, enemyCard);
            await UniTask.WaitUntil(() => _win.IsDone());
            await _playerEffectPresenter.EffectProcess(playerCard);
            int receiveDamage = ApplyDamage(_enemyhp, playerCard, enemyCard);
            _damageTextEffect.WinDamaged(receiveDamage, Matchup(playerCard, enemyCard));
            await PlayDamageTimeline();
        }
    }
}

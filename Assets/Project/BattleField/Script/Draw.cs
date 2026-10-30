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
            // あいこは攻撃力や相性に関係なく、お互いに固定ダメージ。
            // 自分の攻撃 → 相手へのダメージ → 相手の攻撃 → 自分へのダメージ の順に見せる
            await _playerEffectPresenter.EffectProcess(playerCard);
            int enemyReceiveDamage = ApplyFixedDamage(_enemyhp, DrawDamage);
            _damageTextEffect.WinDamaged(enemyReceiveDamage);
            await PlayDamageTimeline();

            await _enemyEffectPresenter.EffectProcess(enemyCard);
            int playerReceiveDamage = ApplyFixedDamage(_playerhp, DrawDamage);
            _damageTextEffect.LoseDamaged(playerReceiveDamage);
            await PlayDamageTimeline();
        }
    }
}

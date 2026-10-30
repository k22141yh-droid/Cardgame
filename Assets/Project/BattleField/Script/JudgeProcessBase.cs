using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

using BattleField.Script.Damage;
using BattleField.Script.HpBar.Model;
using BattleField.Script.TimelineManagge;
using BattleField.Script.Janken;
using Common.MasterData;

namespace BattleField.Script.Battle {
    // Win / Lose / Draw で共通する処理。
    // シリアライズ済みフィールド名はシーン側の参照を保つため変更しないこと。
    public abstract class JudgeProcessBase : MonoBehaviour
    {
        [SerializeField] protected HpBarModel _playerhp;
        [SerializeField] protected HpBarModel _enemyhp;
        [SerializeField] protected TimelineManagger _damageTimeline;
        [SerializeField] protected TextEffect _damageTextEffect;
        [SerializeField] protected JankenSelector _playerJankenSelector;
        [SerializeField] protected JankenSelector _enemyJankenSelector;

        private readonly DamageValue _damageValue = new DamageValue();

        protected void SetJankenOptions(Card playerCard, Card enemyCard) {
            _playerJankenSelector.SetOptions(playerCard.Hand, playerCard.Type);
            _enemyJankenSelector.SetOptions(enemyCard.Hand, enemyCard.Type);
        }

        // ジャンケンの Timeline を始めた直後に呼ぶ。手がぶつかる瞬間に火花を散らして画面を揺らす
        protected void PlayClashEffect(Card playerCard, Card enemyCard, CancellationToken ct) {
            JankenClashEffect.Play(_playerJankenSelector.HandTransform, _enemyJankenSelector.HandTransform,
                playerCard.Type, enemyCard.Type, ct);
        }

        // attacker の威力とタイプから target へのダメージを計算し、HP に反映して実際に引いた値を返す
        protected int ApplyDamage(HpBarModel target, Card attacker, Card receiver) {
            int damage = _damageValue.CalcDamageValue(attacker.Power, attacker.Type, receiver.Type);
            damage = _damageValue.DamageBalance(target.Health, damage);
            target.Health -= damage;
            return damage;
        }

        protected int Matchup(Card attacker, Card receiver) {
            return _damageValue.Matchup(attacker.Type, receiver.Type);
        }

        // 相性や威力に関係なく決まった量のダメージを与え、実際に引いた値を返す
        protected int ApplyFixedDamage(HpBarModel target, int damage) {
            damage = _damageValue.DamageBalance(target.Health, damage);
            target.Health -= damage;
            return damage;
        }

        protected async UniTask PlayDamageTimeline() {
            _damageTimeline.TimelinePlay();
            await UniTask.WaitUntil(() => _damageTimeline.IsDone());
        }
    }
}

using UnityEngine;
using R3;
using BattleField.Script.HpBar.Model;
using BattleField.Script.HpBar.View;

namespace BattleField.Script.HpBar.Presenter {
    public class HpBarPresenter : MonoBehaviour
    {
        public HpBarView _healthView;
        public HpBarModel _model;
        void Start() {
            // 範囲の補正は HpBarModel.Health の setter 側で行う
            _model._hp.Subscribe(value => _healthView.SetRate(_model._healthMax, value));
        }
    }
}

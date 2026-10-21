using UnityEngine;
using R3;
using InGame;

namespace BattleField.Script.HpBar.Model {
    public class HpBarModel : MonoBehaviour
    {
        public int _healthMax;
        public SerializableReactiveProperty<int> _hp = new SerializableReactiveProperty<int>();
        private GameSettings _gameSettings;

        void Awake() {
            _gameSettings = new GameSettings();
            _healthMax = _gameSettings.MaxHp;
            Health = _healthMax;
        }

        public int Health
        {
            get { return _hp.Value; }
            set { _hp.Value = Mathf.Clamp(value, 0, _healthMax); }
        }

        public void Damage(int value) {
            Health -= value;
        }
    }
}

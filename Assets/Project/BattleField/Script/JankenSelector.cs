using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.MasterData;

namespace BattleField.Script.Janken {
    public class JankenSelector : MonoBehaviour
    {
        [SerializeField] private Sprite _fireRock;
        [SerializeField] private Sprite _waterRock;
        [SerializeField] private Sprite _grassRock;
        [SerializeField] private Sprite _fireScissors;
        [SerializeField] private Sprite _waterScissors;
        [SerializeField] private Sprite _grassScissors;
        [SerializeField] private Sprite _firePaper;
        [SerializeField] private Sprite _waterPaper;
        [SerializeField] private Sprite _grassPaper;
        [SerializeField] private Sprite _yoshimoto;
        [SerializeField] private Image _jankenSelector;

        private void Gu_(CardType type) {
            if (type == CardType.Fire) _jankenSelector.sprite = _fireRock;
            else if (type == CardType.Water) _jankenSelector.sprite = _waterRock;
            else _jankenSelector.sprite = _grassRock;
        }
        private void Choki(CardType type) {
            if (type == CardType.Fire) _jankenSelector.sprite = _fireScissors;
            else if (type == CardType.Water) _jankenSelector.sprite = _waterScissors;
            else _jankenSelector.sprite = _grassScissors;
        }
        private void Pa_(CardType type) {
            if (type == CardType.Fire) _jankenSelector.sprite = _firePaper;
            else if (type == CardType.Water) _jankenSelector.sprite = _waterPaper;
            else _jankenSelector.sprite = _grassPaper;
        }
        public void SetOptions(CardHand option, CardType type) {
            switch (option) {
                case CardHand.Rock:
                    Gu_(type);
                    break;
                case CardHand.Scissors:
                    Choki(type);
                    break;
                case CardHand.Paper:
                    Pa_(type);
                    break;
                case CardHand.Yoshimoto:
                    _jankenSelector.sprite = _yoshimoto;
                    break;
            }
        }
    }

}

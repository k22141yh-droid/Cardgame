#nullable enable

using Common.QRCode;
using Cysharp.Threading.Tasks;
using InGame.Player;
using System.Threading;
using Project.Common.Program.Scripts.Audio;
using TMPro;
using UnityEngine;

namespace InGame.Turn
{
    public class TurnPresenter : MonoBehaviour
    {
        private PlayerPresenter _player = null!;
        private PlayerPresenter _opponentPlayer = null!;
        private ScannerModel _scannerModel = null!;
        private int _maxTurn;
        [SerializeField] private TextMeshProUGUI _cardScanMessage = null!;
        [SerializeField] private AudioSource cardScanAudioSource = null!;
        [SerializeField] private DelayLoopAudioSource loadCardVoiceAudioSource = null!;

        public void Initialize(PlayerPresenter player, PlayerPresenter opponentPlayer, ScannerModel scannerModel, int maxTurn)
        {
            _player = player;
            _opponentPlayer = opponentPlayer;
            _scannerModel = scannerModel;
            _maxTurn = maxTurn;
            _cardScanMessage.enabled = false;

            // CancellationToken ct = this.GetCancellationTokenOnDestroy();
            // HandleTurn(ct).Forget();
        }

        public async UniTask HandleTurn(CancellationToken ct)
        {
            _cardScanMessage.enabled = true;
            _cardScanMessage.text = "カードを読み込もう";
            loadCardVoiceAudioSource.gameObject.SetActive(true);
            //_opponentPlayer.SetCurrentCard(new(CardHand.Scissors, CardType.Grass, 1000));
            await UniTask.WaitUntil(() =>
            {
                var result = _player.SetCurrentCard(_scannerModel.QrScanResult);
                return result.Match(
                    _ => true,
                    err =>
                    {
                        if (err == SetCurrentCardError.IncorrectId)
                        {
                            _cardScanMessage.text = "このカードは使えないよ";
                        }
                        else if (err == SetCurrentCardError.DuplicateError)
                        {
                            _cardScanMessage.text = "同じカードは使えないよ";
                        }
                        return false;
                    }
                );
            }, cancellationToken: ct);
            
            loadCardVoiceAudioSource.gameObject.SetActive(false);
            cardScanAudioSource.Play();
            await UniTask.WaitUntil(()=>!cardScanAudioSource.isPlaying, cancellationToken: ct);
            
            _cardScanMessage.text = string.Empty;
            _cardScanMessage.enabled = false;
            //CompareCard();
            // _turn++;
        }

        // hand, type から勝敗と相性を判定し、hp を減らす
        //private void CompareCard()
        //{
        //    UnityEngine.Debug.Log("カードを比較する");
        //}
    }
}
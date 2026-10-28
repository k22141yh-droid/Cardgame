using Common.QRCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Project.RuleExplanation.Program.Scripts.RuleExplanation
{
    public class RuleExplanationPresenter : MonoBehaviour
    {
        // シーンに入った直後の入力(タイトルで押したキーなど)でスキップしないよう、少し待ってから受け付ける
        private const float SkipAcceptDelaySeconds = 1f;

        [SerializeField] private VideoPlayer videoPlayer;

        private ScannerModel _scannerModel;
        private float _startTime;
        private bool _isLeaving;

        public void Initialize(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;
            _startTime = Time.time;
            videoPlayer.loopPointReached += OnVideoComplete;
        }

        // キー・クリック・タッチ、またはカードの読み込みで説明動画を飛ばす
        private void Update()
        {
            if (_isLeaving || _scannerModel == null) return;
            if (Time.time - _startTime < SkipAcceptDelaySeconds) return;

            bool cardScanned = !string.IsNullOrEmpty(_scannerModel.QrScanResult);
            bool pressed = Input.anyKeyDown || Input.touchCount > 0;
            if (cardScanned || pressed)
            {
                GoToBattle();
            }
        }

        private void OnVideoComplete(VideoPlayer _)
        {
            GoToBattle();
        }

        private void GoToBattle()
        {
            if (_isLeaving) return;
            _isLeaving = true;
            videoPlayer.loopPointReached -= OnVideoComplete;
            videoPlayer.Stop();
            SceneManager.LoadScene("BattleField");
        }
    }
}

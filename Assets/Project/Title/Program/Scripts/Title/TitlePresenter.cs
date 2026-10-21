#nullable enable

using System.Threading;
using Common.QRCode;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Title.Program.Scripts.Title
{
    public class TitlePresenter : MonoBehaviour
    {
        private ScannerModel _scannerModel = null!;
        [SerializeField] private AudioSource cardScanAudioSource = null!;
        
        public void Initialize(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;

            var ct = this.GetCancellationTokenOnDestroy();
            A(ct).Forget();
        }
        
        private async UniTaskVoid A(CancellationToken ct)
        {
            while (true)
            {
                if (_scannerModel.QrScanResult != "")
                {
                    cardScanAudioSource.Play();
                    await UniTask.WaitUntil(()=> !cardScanAudioSource.isPlaying, cancellationToken:ct);
                    SceneManager.LoadScene("RuleExplanation");
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
    }
}

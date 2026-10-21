#nullable enable

using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.QRCode
{
    public class Scanner : MonoBehaviour
    {
        private ScannerModel _model = null!;

        public void Initialize(ScannerModel model)
        {
            _model = model;

            _model.CameraTexture.Play();

            this.UpdateAsObservable().Subscribe(_ => UpdateQrScanResult()).AddTo(this);
        }

        private void OnDestroy()
        {
            Debug.Log("destoroy");

            _model.CameraTexture.Stop();
        }

        private void UpdateQrScanResult()
        {
            if (_model.IsScannerEnable)
            {
                _model.QrScanResult = Reader.QRCodeToText(_model.CameraTexture);
                
            }
            if (_model.QrScanResult == "100")
            {
                SceneManager.LoadScene("Title");
            }
        }
    }
}
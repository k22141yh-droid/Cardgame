#if !EXCLUDE_UNITY_DEBUG_SHEET
#nullable enable

using Common.QRCode;
using UnityEngine;
using UnityEngine.UI;

namespace Project.InGame.Program.Scripts.DebugSheet
{
    public sealed class InGamePage : MonoBehaviour
    {
        [SerializeField] private RawImage qrCodeScannerScreen = null!;
        private ScannerModel _scannerModel = null!;

        public void Initialize(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;

            var rootPage = UnityDebugSheet.Runtime.Core.Scripts.DebugSheet.Instance.GetOrCreateInitialPage("DebugPage");

            rootPage.AddPageLinkButton<ScannerPage>("InGame:QrCodeScanner", onLoad: x => x.page.Setup(qrCodeScannerScreen, _scannerModel));
        }
    }
}

#endif
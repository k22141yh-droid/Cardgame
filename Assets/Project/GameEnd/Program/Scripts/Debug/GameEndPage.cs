#if !EXCLUDE_UNITY_DEBUG_SHEET
#nullable enable

using Common.QRCode;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Project.GameEnd.Program.Scripts.Debug
{
    public sealed class GameEndPage : MonoBehaviour
    {
        [SerializeField] private RawImage qrCodeScannerScreen = null!;
        private ScannerModel _scannerModel = null!;

        public void Initialize(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;

            var rootPage = DebugSheet.Instance.GetOrCreateInitialPage("DebugPage");

            rootPage.AddPageLinkButton<ScannerPage>("GameEnd:QrCodeScanner", onLoad: x => x.page.Setup(qrCodeScannerScreen, _scannerModel));
        }
    }
}

#endif
#if !EXCLUDE_UNITY_DEBUG_SHEET
#nullable enable

using Common.QRCode;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Title.Program.Scripts.Debug
{
    public class MainPage : MonoBehaviour
    {
        [SerializeField] private RawImage qrCodeScannerScreen = null!;

        public void Initialize(ScannerModel scannerModel)
        {
            var rootPage = DebugSheet.Instance.GetOrCreateInitialPage("DebugMenu");
            
            rootPage.AddPageLinkButton<ScannerPage>("Title:QrCodeScanner", onLoad: x => x.page.Setup(qrCodeScannerScreen, scannerModel));
        }
    }
}

#endif

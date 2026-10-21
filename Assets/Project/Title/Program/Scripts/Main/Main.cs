#nullable enable

using Common.QRCode;
using Project.Title.Program.Scripts.Debug;
using Project.Title.Program.Scripts.Title;
using UnityEngine;

namespace Project.Title.Program.Scripts.Main
{
    public class Main : MonoBehaviour
    {
        private ScannerModel _scannerModel = null!;
        [SerializeField] private Scanner scanner = null!;
        [SerializeField] private MainPage debugPage = null!;
        [SerializeField] private TitlePresenter title = null!;

        private void Start()
        {
            _scannerModel = new ScannerModel();
        
            scanner.Initialize(_scannerModel);
            title.Initialize(_scannerModel);
        
#if !EXCLUDE_UNITY_DEBUG_SHEET
            debugPage.Initialize(_scannerModel);
#endif    
        }
    }
}

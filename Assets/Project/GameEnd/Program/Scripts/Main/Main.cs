using Common.QRCode;
using Project.GameEnd.Program.Scripts.Debug;
using Project.GameEnd.Program.Scripts.GameEnd;
using UnityEngine;

namespace Project.GameEnd.Program.Scripts.Main
{
    public class Main : MonoBehaviour
    {
        private ScannerModel _scannerModel = null!;
        [SerializeField] private Scanner scanner = null!;
        // [SerializeField] private GameEndPage debugPage = null!;
        [SerializeField] private GameEndPresenter ruleExplanation = null!;

        private void Start()
        {
            _scannerModel = new ScannerModel();
        
            scanner.Initialize(_scannerModel);
            ruleExplanation.Initialize();
        
// #if !EXCLUDE_UNITY_DEBUG_SHEET
//             debugPage.Initialize(_scannerModel);
// #endif    
        }
    }
}
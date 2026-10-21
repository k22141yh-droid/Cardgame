using Common.QRCode;
using Project.RuleExplanation.Program.Scripts.Debug;
using Project.RuleExplanation.Program.Scripts.RuleExplanation;
using UnityEngine;

namespace Project.RuleExplanation.Program.Scripts.Main
{
    public class Main : MonoBehaviour
    {
        private ScannerModel _scannerModel = null!;
        [SerializeField] private Scanner scanner = null!;
        // [SerializeField] private RuleExplanationPage debugPage = null!;
        [SerializeField] private RuleExplanationPresenter ruleExplanation = null!;

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

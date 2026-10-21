#if !EXCLUDE_UNITY_DEBUG_SHEET
#nullable enable

using System.Threading.Tasks;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.Cells;
using UnityEngine.UI;

namespace Common.QRCode
{
    public sealed class ScannerPage : DefaultDebugPageBase
    {
        protected override string Title => "QrCodeScanner";
        private RawImage _scannerScreen = null!;
        private ScannerModel _model = null!;
        private InputFieldCellModel _scanResultCell = null!;
        private int _scanResultId;

        private void Update()
        {
            _scanResultCell.Value = _model.QrScanResult;
            RefreshDataAt(_scanResultId);
        }

        public void Setup(RawImage scannerScreen, ScannerModel model)
        {
            _scannerScreen = scannerScreen;
            _model = model;

            _scannerScreen.rectTransform.sizeDelta = new(_model.Width, _model.Height);
            _scannerScreen.texture = _model.CameraTexture;
        }

        public override Task Initialize()
        {
            AddSwitch(_model.IsScannerEnable, "IsScannerEnable", valueChanged: x => { _model.IsScannerEnable = x; });

            _scanResultCell = new(false);
            _scanResultCell.CellTexts.Text = "QrScanResult";
            _scanResultCell.Value = _model.QrScanResult;
            _scanResultCell.Placeholder = "値は空です";
            _scanResultCell.ValueChanged += x => { _model.QrScanResult = x; };
            _scanResultId = AddInputField(_scanResultCell);

            AddSwitch(false, "QrCodeScannerScreenを表示する", valueChanged: x => { _scannerScreen.gameObject.SetActive(x); });

            AddLabel("Width", subText: _model.Width.ToString());

            AddLabel("Height", subText: _model.Height.ToString());

            return Task.CompletedTask;
        }
    }
}

#endif
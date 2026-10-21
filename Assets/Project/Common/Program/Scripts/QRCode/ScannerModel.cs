#nullable enable

using UnityEngine;

namespace Common.QRCode
{
    public class ScannerModel
    {
        public bool IsScannerEnable { get; set; } = true;
        public string QrScanResult { get; set; } = string.Empty;
        public WebCamTexture CameraTexture { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ScannerModel()
        {
            Width = 500;
            Height = 500;
            CameraTexture = new(WebCamTexture.devices[0].name, Width, Height)
            {
                autoFocusPoint = new Vector2(Width * 0.2f, Height * 0.2f)
            };
        }
    }
}
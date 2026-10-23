#nullable enable

using UnityEngine;

namespace Common.QRCode
{
    public class ScannerModel
    {
        public bool IsScannerEnable { get; set; } = true;
        public string QrScanResult { get; set; } = string.Empty;
        /// <summary>カメラが搭載されていない場合は null</summary>
        public WebCamTexture? CameraTexture { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ScannerModel()
        {
            Width = 500;
            Height = 500;

            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogWarning("カメラが見つかりません。QRコードの読み取りは無効です。");
                return;
            }

            CameraTexture = new(devices[0].name, Width, Height)
            {
                autoFocusPoint = new Vector2(Width * 0.2f, Height * 0.2f)
            };
        }
    }
}
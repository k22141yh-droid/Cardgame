#nullable enable

using System.Collections.Generic;
using UnityEngine;
using ZXing;

namespace Common.QRCode
{
    public static class Reader
    {
        private static readonly BarcodeReader _reader = new()
        {
            AutoRotate = true,
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };

        public static string QRCodeToText(WebCamTexture texture)
        {
            // カメラ起動直後は 16x16 のダミーサイズなので読み取らない
            if (texture.width <= 16 || !texture.didUpdateThisFrame)
            {
                return string.Empty;
            }

            var result = _reader.Decode(texture.GetPixels32(), texture.width, texture.height);

            return result != null ? result.Text : string.Empty;
        }
    }
}

#nullable enable

using UnityEngine;
using ZXing;

namespace Common.QRCode
{
    public static class Reader
    {
        public static string QRCodeToText(WebCamTexture texture)
        {
            BarcodeReader reader = new();
            var result = reader.Decode(texture.GetPixels32(), texture.width, texture.height);

            return result != null ? result.Text : string.Empty;
        }
    }
}
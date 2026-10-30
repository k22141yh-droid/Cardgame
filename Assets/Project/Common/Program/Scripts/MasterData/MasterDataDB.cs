#nullable enable

using Generated.MasterData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.MasterData
{
    public static class MasterDataDB
    {
        #if UNITY_EDITOR
        public static readonly MemoryDatabase DB = new(
            AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Project/Common/Program/MasterData/master_data.bytes").bytes
            );
        #else
        public static readonly MemoryDatabase DB = new(
            Resources.Load<TextAsset>("MasterData/master_data").bytes
            );
        #endif
    }
}
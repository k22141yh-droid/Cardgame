using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Project.Common.Program.Scripts.Editor
{
    // Title の文字に使うフォント(Zen Kaku Gothic New Bold)の TextMeshPro 用フォントアセットと「白文字+縁取り」マテリアルを作り、
    // Title シーンの DescriptionText(カードを読み込んでゲームスタート!)に適用する。
    // メニュー「Tools > Cardgame > Title の文字のフォントを適用する」から 1 回実行する。
    public static class TitleFontSetup
    {
        private const string FontDirectory = "Assets/Project/Common/Design/Fonts/ZenKakuGothicNew";
        private const string FontPath = FontDirectory + "/ZenKakuGothicNew-Bold.ttf";
        private const string FontAssetPath = FontDirectory + "/ZenKakuGothicNew-Bold-SDF.asset";
        private const string OutlineMaterialPath = FontDirectory + "/ZenKakuGothicNew-Bold-SDF-WhiteOutline.mat";
        private const string TitleScenePath = "Assets/Project/Title/Program/Scenes/Title.unity";
        private const string TargetObjectName = "DescriptionText";

        // 縁取りが太くても欠けないよう、パディングを大きめにとる
        private const int SamplingPointSize = 90;
        private const int AtlasPadding = 12;

        private static readonly Color FaceColor = Color.white;
        private static readonly Color OutlineColor = Color.black;
        private const float OutlineWidth = 0.42f;

        [MenuItem("Tools/Cardgame/Title の文字のフォントを適用する")]
        public static void Apply()
        {
            var fontAsset = LoadOrCreateFontAsset();
            if (fontAsset == null) return;

            var material = LoadOrCreateOutlineMaterial(fontAsset);
            ApplyToTitle(fontAsset, material);
        }

        private static TMP_FontAsset LoadOrCreateFontAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null) return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogError($"フォントが見つかりません: {FontPath}");
                return null;
            }

            // 日本語は文字数が多いので、使った文字だけを追加していく Dynamic にする
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
            fontAsset.name = "ZenKakuGothicNew-Bold-SDF";

            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

            fontAsset.atlasTextures[0].name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static Material LoadOrCreateOutlineMaterial(TMP_FontAsset fontAsset)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            if (material == null)
            {
                material = new Material(fontAsset.material) { name = "ZenKakuGothicNew-Bold-SDF-WhiteOutline" };
                AssetDatabase.CreateAsset(material, OutlineMaterialPath);
            }

            material.SetColor(ShaderUtilities.ID_FaceColor, FaceColor);
            material.SetColor(ShaderUtilities.ID_OutlineColor, OutlineColor);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, OutlineWidth);

            // 影は付けない
            material.DisableKeyword(ShaderUtilities.Keyword_Underlay);

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void ApplyToTitle(TMP_FontAsset fontAsset, Material material)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
            TMP_Text target = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (text.gameObject.name == TargetObjectName)
                    {
                        target = text;
                        break;
                    }
                }
                if (target != null) break;
            }

            if (target == null)
            {
                Debug.LogError($"{TitleScenePath} に {TargetObjectName} が見つかりません");
                return;
            }

            // 表示する文字を先にフォントアセットへ登録しておく
            fontAsset.TryAddCharacters(target.text, out _);
            EditorUtility.SetDirty(fontAsset);

            Undo.RecordObject(target, "Title の文字のフォントを変更");
            target.font = fontAsset;
            target.fontSharedMaterial = material;
            target.color = Color.white;
            EditorUtility.SetDirty(target);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = target.gameObject;
            Debug.Log($"{TargetObjectName} を Zen Kaku Gothic New Bold(白文字+縁取り)に変更しました");
        }
    }
}

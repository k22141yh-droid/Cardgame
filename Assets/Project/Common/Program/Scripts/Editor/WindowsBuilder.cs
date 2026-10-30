using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Project.Common.Program.Scripts.Editor
{
    // Unity なしで遊べるよう、Windows 用の実行ファイル(CardGame.exe)を作る。
    // メニュー「Tools > Cardgame > Windows 用にビルドする」から実行する。
    // 出力先はプロジェクト直下の Build/Windows(Git には含めない)。
    public static class WindowsBuilder
    {
        private const string OutputDirectory = "Build/Windows";
        private const string ExeName = "CardGame.exe";

        [MenuItem("Tools/Cardgame/Windows 用にビルドする")]
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("ビルド", "Build Settings にシーンが登録されていません。", "OK");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(OutputDirectory, ExeName),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            var folder = Path.GetFullPath(OutputDirectory);

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"ビルド完了: {folder} ({summary.totalSize / (1024 * 1024)} MB)");
                EditorUtility.RevealInFinder(Path.Combine(folder, ExeName));
            }
            else
            {
                Debug.LogError($"ビルド失敗: {summary.result}(エラー {summary.totalErrors} 件)。Console のエラーを確認してください。");
                EditorUtility.DisplayDialog("ビルド", "ビルドに失敗しました。Console のエラーを確認してください。", "OK");
            }
        }
    }
}

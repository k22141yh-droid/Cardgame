using System.Threading;
using Cysharp.Threading.Tasks;
using Project.BattleField.Script.GameEnd;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Project.GameEnd.Program.Scripts.GameEnd
{
    public class GameEndPresenter : MonoBehaviour
    {
        [SerializeField] private VideoPlayer winVideo;
        [SerializeField] private VideoPlayer loseVideo;
        [SerializeField] private VideoPlayer ending;

        public void Initialize()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            PlayGameEndVideo(ct).Forget();
        }

        private async UniTaskVoid PlayGameEndVideo(CancellationToken ct)
        {
            switch (GameEndSceneData.GameResult)
            {
                case GameResult.Win:
                    await ActiveAndPrepareVideo(ct, winVideo);
                    break;
                case GameResult.Draw:
                case GameResult.Lose:
                default:
                    await ActiveAndPrepareVideo(ct, loseVideo);
                    break;
            }

            ending.gameObject.SetActive(true);
            await UniTask.WaitUntil(() => ending.isPaused, cancellationToken: ct);

             SceneManager.LoadScene("Title");
        }

        private static async UniTask ActiveAndPrepareVideo(CancellationToken ct, VideoPlayer vp)
        {
            vp.gameObject.SetActive(true);
            await UniTask.WaitUntil(() => vp.isPaused, cancellationToken: ct);
            await UniTask.DelayFrame(60, PlayerLoopTiming.FixedUpdate, ct);
            vp.gameObject.SetActive(false);
        }
    }
}
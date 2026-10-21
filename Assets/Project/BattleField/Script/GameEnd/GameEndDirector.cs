#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Project.BattleField.Script.GameEnd
{
    public class GameEndDirector : MonoBehaviour
    {
        [SerializeField] private PlayableDirector endDirector = null!;
        [SerializeField] private PlayableDirector winDirector = null!;
        [SerializeField] private PlayableDirector drawDirector = null!;
        [SerializeField] private PlayableDirector loseDirector = null!;
        [SerializeField] private PlayableDirector announceDirector = null!;

        public async UniTask PlayDirector(GameResult gameResult, CancellationToken ct)
        {
            endDirector.Play();
            await UniTask.WaitUntil(() => endDirector.state == PlayState.Paused, cancellationToken: ct);

            if (gameResult == GameResult.Win)
            {
                winDirector.Play();
                await UniTask.WaitUntil(() => winDirector.state == PlayState.Paused, cancellationToken: ct);
            }
            else if (gameResult == GameResult.Draw)
            {
                drawDirector.Play();
                await UniTask.WaitUntil(() => drawDirector.state == PlayState.Paused, cancellationToken: ct);
            }
            else
            {
                loseDirector.Play();
                await UniTask.WaitUntil(() => loseDirector.state == PlayState.Paused, cancellationToken: ct);
            }

            announceDirector.Play();
            await UniTask.WaitUntil(() => announceDirector.state == PlayState.Paused, cancellationToken: ct);
        }
    }

    public enum GameResult
    {
        Win,
        Draw,
        Lose
    }
}
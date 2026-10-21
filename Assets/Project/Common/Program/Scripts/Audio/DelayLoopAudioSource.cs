#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Common.Program.Scripts.Audio
{
    public class DelayLoopAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource = null!;
        [SerializeField] private int loopDelay;

        private void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            DelayLoop(ct).Forget();
        }

        private async UniTaskVoid DelayLoop(CancellationToken ct)
        {
            while (true)
            {
                if (audioSource.gameObject.activeSelf)
                {
                    audioSource.Play();
                    await UniTask.WaitUntil(()=>!audioSource.isPlaying, cancellationToken: ct);
                    await UniTask.DelayFrame(loopDelay, PlayerLoopTiming.FixedUpdate, cancellationToken: ct);
                }
                else
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);
                }
            }
        }
    }
}
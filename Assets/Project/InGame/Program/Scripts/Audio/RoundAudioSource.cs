#nullable enable

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.InGame.Program.Scripts.Audio
{
    public class RoundAudioSource: MonoBehaviour
    {
        [SerializeField] private AudioClip round1 = null!;
        [SerializeField] private AudioClip round2 = null!;
        [SerializeField] private AudioClip round3 = null!;
        [SerializeField] private AudioSource audioSource = null!;

        public void Play(int turn)
        {
            audioSource.clip = turn switch
            {
                1 => round1,
                2 => round2,
                _ => round3
            };
            audioSource.Play();
        }
    }
}
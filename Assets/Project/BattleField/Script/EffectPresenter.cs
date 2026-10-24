using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using Common.MasterData;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using BattleField.Script.TimelineManagge;

namespace BattleField.Script.Effect
{
    // カードごとの攻撃動画。Level は威力 2000 → 3 / 1000 → 2 / 500 → 1(ヨシモトは 0)
    [Serializable]
    public class AttackVideoEntry
    {
        public CardType Type;
        public CardHand Hand;
        public int Level;
        public VideoClip Clip;
    }

    public class EffectPresenter : MonoBehaviour
    {
        // --- Level 3 ---
        [SerializeField] TimelineManagger _level3FireRockEffect;
        [SerializeField] TimelineManagger _level3FireScissorsEffect;
        [SerializeField] TimelineManagger _level3FirePaperEffect;
        [SerializeField] TimelineManagger _level3WaterRockEffect;
        [SerializeField] TimelineManagger _level3WaterScissorsEffect;
        [SerializeField] TimelineManagger _level3WaterPaperEffect;
        [SerializeField] TimelineManagger _level3GrassRockEffect;
        [SerializeField] TimelineManagger _level3GrassScissorsEffect;
        [SerializeField] TimelineManagger _level3GrassPaperEffect;

        // --- Level 2 ---
        [SerializeField] TimelineManagger _level2FireRockEffect;
        [SerializeField] TimelineManagger _level2FireScissorsEffect;
        [SerializeField] TimelineManagger _level2FirePaperEffect;
        [SerializeField] TimelineManagger _level2WaterRockEffect;
        [SerializeField] TimelineManagger _level2WaterScissorsEffect;
        [SerializeField] TimelineManagger _level2WaterPaperEffect;
        [SerializeField] TimelineManagger _level2GrassRockEffect;
        [SerializeField] TimelineManagger _level2GrassScissorsEffect;
        [SerializeField] TimelineManagger _level2GrassPaperEffect;

        // --- Level 1 ---
        [SerializeField] TimelineManagger _level1FireRockEffect;
        [SerializeField] TimelineManagger _level1FireScissorsEffect;
        [SerializeField] TimelineManagger _level1FirePaperEffect;
        [SerializeField] TimelineManagger _level1WaterRockEffect;
        [SerializeField] TimelineManagger _level1WaterScissorsEffect;
        [SerializeField] TimelineManagger _level1WaterPaperEffect;
        [SerializeField] TimelineManagger _level1GrassRockEffect;
        [SerializeField] TimelineManagger _level1GrassScissorsEffect;
        [SerializeField] TimelineManagger _level1GrassPaperEffect;

        // 該当するカードの動画が登録されていれば、Timeline の代わりに動画を再生する
        [SerializeField] List<AttackVideoEntry> _attackVideos = new List<AttackVideoEntry>();

        private VideoPlayer _videoPlayer;
        private GameObject _videoCanvas;
        private RawImage _videoImage;
        private RenderTexture _videoTexture;

        // 威力 → エフェクトレベル
        private const int PowerLevel3 = 2000;
        private const int PowerLevel2 = 1000;
        private const int PowerLevel1 = 500;

        private const int HandCount = 3;

        private static int PowerToLevel(int power) => power switch
        {
            PowerLevel3 => 3,
            PowerLevel2 => 2,
            PowerLevel1 => 1,
            _ => 0
        };

        // 添字: タイプ(Fire, Water, Grass) * HandCount + 手(Rock, Scissors, Paper)
        private TimelineManagger[] GetLevelEffects(int level) => level switch
        {
            3 => new[] {
                _level3FireRockEffect, _level3FireScissorsEffect, _level3FirePaperEffect,
                _level3WaterRockEffect, _level3WaterScissorsEffect, _level3WaterPaperEffect,
                _level3GrassRockEffect, _level3GrassScissorsEffect, _level3GrassPaperEffect },
            2 => new[] {
                _level2FireRockEffect, _level2FireScissorsEffect, _level2FirePaperEffect,
                _level2WaterRockEffect, _level2WaterScissorsEffect, _level2WaterPaperEffect,
                _level2GrassRockEffect, _level2GrassScissorsEffect, _level2GrassPaperEffect },
            1 => new[] {
                _level1FireRockEffect, _level1FireScissorsEffect, _level1FirePaperEffect,
                _level1WaterRockEffect, _level1WaterScissorsEffect, _level1WaterPaperEffect,
                _level1GrassRockEffect, _level1GrassScissorsEffect, _level1GrassPaperEffect },
            _ => null
        };

        private TimelineManagger FindEffect(Card card, int level)
        {
            var effects = GetLevelEffects(level);
            if (effects == null) return null;

            int typeIndex = card.Type switch
            {
                CardType.Fire => 0,
                CardType.Water => 1,
                CardType.Grass => 2,
                _ => -1
            };
            int handIndex = card.Hand switch
            {
                CardHand.Rock => 0,
                CardHand.Scissors => 1,
                CardHand.Paper => 2,
                _ => -1
            };
            if (typeIndex < 0 || handIndex < 0) return null;

            return effects[typeIndex * HandCount + handIndex];
        }

        private VideoClip FindVideo(Card card, int level)
        {
            foreach (var entry in _attackVideos)
            {
                if (entry.Clip != null && entry.Type == card.Type && entry.Hand == card.Hand && entry.Level == level)
                {
                    return entry.Clip;
                }
            }
            return null;
        }

        // 画面全体を覆う Canvas に動画を映して、再生が終わるまで待つ。
        // シーン側の配置を増やさないよう、必要になった時点で実行時に生成する。
        private void EnsureVideoSurface()
        {
            if (_videoPlayer != null) return;

            _videoCanvas = new GameObject("AttackVideoCanvas");
            _videoCanvas.transform.SetParent(transform, false);
            var canvas = _videoCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var imageObject = new GameObject("AttackVideoImage");
            imageObject.transform.SetParent(_videoCanvas.transform, false);
            _videoImage = imageObject.AddComponent<RawImage>();
            _videoImage.raycastTarget = false;
            var rect = _videoImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _videoPlayer = _videoCanvas.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = false;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            // 非アクティブな GameObject の VideoPlayer は Prepare / Play できないため、
            // Canvas はアクティブのまま、映像の表示だけを切り替える
            _videoImage.enabled = false;
        }

        private async UniTask PlayVideo(VideoClip clip)
        {
            EnsureVideoSurface();
            var ct = this.GetCancellationTokenOnDestroy();

            ReleaseVideoTexture();
            _videoTexture = new RenderTexture((int)clip.width, (int)clip.height, 0);
            _videoPlayer.targetTexture = _videoTexture;
            _videoImage.texture = _videoTexture;
            _videoPlayer.clip = clip;

            bool finished = false;
            void OnFinished(VideoPlayer _) => finished = true;
            void OnError(VideoPlayer _, string message)
            {
                Debug.LogError($"攻撃動画を再生できません ({clip.name}): {message}");
                finished = true;
            }
            _videoPlayer.loopPointReached += OnFinished;
            _videoPlayer.errorReceived += OnError;

            _videoPlayer.Prepare();
            await UniTask.WaitUntil(() => _videoPlayer.isPrepared || finished, cancellationToken: ct);

            if (!finished)
            {
                _videoImage.enabled = true;
                _videoPlayer.Play();
                await UniTask.WaitUntil(() => finished, cancellationToken: ct);
            }

            _videoPlayer.loopPointReached -= OnFinished;
            _videoPlayer.errorReceived -= OnError;
            _videoPlayer.Stop();
            _videoImage.enabled = false;
            ReleaseVideoTexture();
        }

        private void ReleaseVideoTexture()
        {
            if (_videoTexture == null) return;
            _videoTexture.Release();
            Destroy(_videoTexture);
            _videoTexture = null;
        }

        private void OnDestroy()
        {
            ReleaseVideoTexture();
        }

        public async UniTask EffectProcess(Card card)
        {
            int level = PowerToLevel(card.Power);

            var clip = FindVideo(card, level);
            if (clip != null)
            {
                Debug.Log($"[{name}] 攻撃動画を再生: {clip.name} (ID {card.Id})");
                await PlayVideo(clip);
                return;
            }

            if (card.Type == CardType.Yoshimoto)
            {
                // 特殊カード処理（現行仕様を維持）
                _level3FireRockEffect?.TimelinePlay();
                _level3WaterRockEffect?.TimelinePlay();
                _level3GrassRockEffect?.TimelinePlay();
                await UniTask.WaitUntil(() => _level3GrassRockEffect == null || _level3GrassRockEffect.IsDone());
                return;
            }

            if (level == 0) return;

            TimelineManagger targetEffect = FindEffect(card, level);

            if (targetEffect != null)
            {
                targetEffect.TimelinePlay();
                await UniTask.WaitUntil(() => targetEffect.IsDone());
            }
        }
    }
}
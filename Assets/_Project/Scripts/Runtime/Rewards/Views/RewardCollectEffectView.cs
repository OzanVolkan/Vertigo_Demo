using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;

namespace Vertigo.Rewards.Views
{
    public class RewardCollectEffectView : MonoBehaviour
    {
        [Header("Tier Appearance")]
        [SerializeField] private Sprite bronzeFrameSprite;
        [SerializeField] private Sprite silverFrameSprite;
        [SerializeField] private Sprite goldFrameSprite;

        [Header("Container & Sizing")]
        [SerializeField] private RectTransform ui_particle_container;
        [SerializeField] private Vector2 particleSize = new Vector2(64f, 64f);
        [SerializeField] private Vector2 iconSize = new Vector2(48f, 48f);

        [Header("Particle Count")]
        [SerializeField, Range(3, 8)] private int minParticles = 4;
        [SerializeField, Range(3, 10)] private int maxParticles = 6;

        [Header("Burst Settings")]
        [SerializeField] private float burstMinRadius = 110f;
        [SerializeField] private float burstMaxRadius = 160f;
        [SerializeField] private float burstDuration = 0.32f;
        [SerializeField] private float burstPauseDuration = 0.12f;

        [Header("Flight Settings")]
        [SerializeField] private float flyDuration = 0.42f;
        [SerializeField] private float staggerDelay = 0.08f;
        [SerializeField] private float countUpDuration = 0.45f;

        private readonly List<GameObject> _activeParticles = new();
        private readonly List<Sequence> _activeSequences = new();

        private void Awake()
        {
            if (ui_particle_container != null) return;

            ui_particle_container = GetComponent<RectTransform>();
            if (ui_particle_container == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                if (canvas != null) ui_particle_container = canvas.GetComponent<RectTransform>();
            }
        }

        private void OnDestroy() => ClearActiveParticles();

        public Sprite GetFrameSprite(WheelType wheelType) => wheelType switch
        {
            WheelType.BronzeNormal => bronzeFrameSprite != null ? bronzeFrameSprite : goldFrameSprite,
            WheelType.SilverSafe => silverFrameSprite != null ? silverFrameSprite : goldFrameSprite,
            _ => goldFrameSprite != null ? goldFrameSprite : (silverFrameSprite != null ? silverFrameSprite : bronzeFrameSprite)
        };

        public void PlayCollectEffect(
            Sprite rewardIcon,
            Vector3 startWorldPos,
            InventoryRewardSlotView targetSlot,
            int oldAmount,
            int newAmount,
            WheelType wheelType,
            Action onComplete)
        {
            if (targetSlot == null)
            {
                onComplete?.Invoke();
                return;
            }

            GameEvents.FireSfx(SoundEffectType.RewardParticleBurst);

            Sprite frameSprite = GetFrameSprite(wheelType);
            int count = UnityEngine.Random.Range(minParticles, maxParticles + 1);
            int landedCount = 0;
            bool hasPopped = false;

            Transform parentTransform = ui_particle_container != null ? ui_particle_container : transform;
            Vector3 startLocalPos = ToLocalXY(parentTransform, startWorldPos);
            Vector3 targetLocalPos = ToLocalXY(parentTransform, targetSlot.IconWorldPosition);

            for (int i = 0; i < count; i++)
            {
                int particleIndex = i;
                GameObject pObj = CreateParticleObject(rewardIcon, frameSprite, parentTransform);
                pObj.transform.localPosition = startLocalPos;
                pObj.transform.localScale = Vector3.zero;
                _activeParticles.Add(pObj);

                float rad = ((360f / count) * particleIndex + UnityEngine.Random.Range(-25f, 25f)) * Mathf.Deg2Rad;
                float dist = UnityEngine.Random.Range(burstMinRadius, burstMaxRadius);
                Vector3 burstLocalPos = startLocalPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * dist;
                float tiltAngle = UnityEngine.Random.Range(-20f, 20f);

                Sequence seq = DOTween.Sequence();
                _activeSequences.Add(seq);

                // 1. Burst Outwards
                seq.Append(pObj.transform.DOScale(Vector3.one, burstDuration).SetEase(Ease.OutBack, 1.4f));
                seq.Join(pObj.transform.DOLocalMove(burstLocalPos, burstDuration).SetEase(Ease.OutCubic));
                seq.Join(pObj.transform.DORotate(new Vector3(0f, 0f, tiltAngle), burstDuration).SetEase(Ease.OutQuad));

                // 2. Staggered Pause & Flight to Inventory Slot
                seq.AppendInterval(burstPauseDuration + (particleIndex * staggerDelay));
                seq.Append(pObj.transform.DOLocalMove(targetLocalPos, flyDuration).SetEase(Ease.InCubic));
                seq.Join(pObj.transform.DORotate(Vector3.zero, flyDuration).SetEase(Ease.InQuad));

                // 3. Slot Impact
                seq.AppendCallback(() =>
                {
                    GameEvents.FireSfx(SoundEffectType.RewardSlotHit, 1f + (particleIndex * 0.03f));
                    if (!hasPopped && targetSlot != null)
                    {
                        hasPopped = true;
                        targetSlot.PlayPop();
                        targetSlot.AnimateCountUp(oldAmount, newAmount, countUpDuration);
                    }
                });

                // 4. Shrink & Cleanup
                seq.Append(pObj.transform.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InQuad));
                seq.OnComplete(() =>
                {
                    landedCount++;
                    _activeParticles.Remove(pObj);
                    _activeSequences.Remove(seq);
                    if (pObj != null) Destroy(pObj);

                    if (landedCount >= count)
                    {
                        float waitExtra = Mathf.Max(0.08f, countUpDuration - 0.15f);
                        DOVirtual.DelayedCall(waitExtra, () => onComplete?.Invoke());
                    }
                });
            }
        }

        private GameObject CreateParticleObject(Sprite rewardIcon, Sprite frameSprite, Transform parent)
        {
            Image frameImg = CreateCenteredImage("ui_particle_reward", parent, particleSize, frameSprite, false);
            frameImg.type = frameSprite != null && frameSprite.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;

            CreateCenteredImage("ui_icon", frameImg.transform, iconSize, rewardIcon, true);
            return frameImg.gameObject;
        }

        private static Image CreateCenteredImage(string name, Transform parent, Vector2 size, Sprite sprite, bool preserveAspect)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return img;
        }

        private static Vector3 ToLocalXY(Transform parent, Vector3 worldPos)
        {
            Vector3 local = parent.InverseTransformPoint(worldPos);
            local.z = 0f;
            return local;
        }

        public void ClearActiveParticles()
        {
            foreach (var seq in _activeSequences) seq?.Kill();
            _activeSequences.Clear();

            foreach (var p in _activeParticles)
            {
                if (p != null) Destroy(p);
            }
            _activeParticles.Clear();
        }
    }
}

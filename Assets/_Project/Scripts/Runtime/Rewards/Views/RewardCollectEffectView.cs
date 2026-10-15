using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;

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

        private readonly List<GameObject> _activeParticles = new List<GameObject>();
        private readonly List<Sequence> _activeSequences = new List<Sequence>();

        private void Awake()
        {
            if (ui_particle_container == null)
            {
                ui_particle_container = GetComponent<RectTransform>();
                if (ui_particle_container == null)
                {
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        ui_particle_container = canvas.GetComponent<RectTransform>();
                    }
                }
            }
        }

        public Sprite GetFrameSprite(WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.BronzeNormal:
                    return bronzeFrameSprite != null ? bronzeFrameSprite : goldFrameSprite;
                case WheelType.SilverSafe:
                    return silverFrameSprite != null ? silverFrameSprite : goldFrameSprite;
                case WheelType.GoldenSuper:
                default:
                    return goldFrameSprite != null ? goldFrameSprite : (silverFrameSprite != null ? silverFrameSprite : bronzeFrameSprite);
            }
        }

        public void PlayCollectEffect(
            Sprite rewardIcon,
            Vector3 startWorldPos,
            InventoryRewardSlotView targetSlot,
            int oldAmount,
            int newAmount,
            Action onComplete)
        {
            PlayCollectEffect(rewardIcon, startWorldPos, targetSlot, oldAmount, newAmount, WheelType.BronzeNormal, onComplete);
        }

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

            Sprite frameSprite = GetFrameSprite(wheelType);
            int count = UnityEngine.Random.Range(minParticles, maxParticles + 1);
            int landedCount = 0;
            bool hasPopped = false;

            Transform parentTransform = ui_particle_container != null ? ui_particle_container : transform;

            // Convert world positions to Canvas local space so effect scales 1:1 with CanvasScaler across all resolutions
            Vector3 startLocalPos = parentTransform.InverseTransformPoint(startWorldPos);
            startLocalPos.z = 0f;

            Vector3 targetLocalPos = targetSlot != null 
                ? parentTransform.InverseTransformPoint(targetSlot.IconWorldPosition) 
                : startLocalPos;
            targetLocalPos.z = 0f;

            for (int i = 0; i < count; i++)
            {
                int particleIndex = i;
                GameObject pObj = CreateParticleObject(rewardIcon, frameSprite, parentTransform);
                pObj.transform.localPosition = startLocalPos;
                pObj.transform.localScale = Vector3.zero;
                _activeParticles.Add(pObj);

                // Scatter angle & radius (in Canvas reference units)
                float angle = (360f / count) * particleIndex + UnityEngine.Random.Range(-25f, 25f);
                float rad = angle * Mathf.Deg2Rad;
                float dist = UnityEngine.Random.Range(burstMinRadius, burstMaxRadius);
                Vector3 burstOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * dist;
                Vector3 burstLocalPos = startLocalPos + burstOffset;
                burstLocalPos.z = 0f;
                float tiltAngle = UnityEngine.Random.Range(-20f, 20f);

                Sequence seq = DOTween.Sequence();
                _activeSequences.Add(seq);

                // 1. Burst Outwards (Local Canvas space)
                seq.Append(pObj.transform.DOScale(Vector3.one, burstDuration).SetEase(Ease.OutBack, 1.4f));
                seq.Join(pObj.transform.DOLocalMove(burstLocalPos, burstDuration).SetEase(Ease.OutCubic));
                seq.Join(pObj.transform.DORotate(new Vector3(0, 0, tiltAngle), burstDuration).SetEase(Ease.OutQuad));

                // 2. Pause in burst formation + stagger
                seq.AppendInterval(burstPauseDuration + (particleIndex * staggerDelay));

                // 3. Flight to target inventory slot (Local Canvas space)
                seq.Append(pObj.transform.DOLocalMove(targetLocalPos, flyDuration).SetEase(Ease.InCubic));
                seq.Join(pObj.transform.DORotate(Vector3.zero, flyDuration).SetEase(Ease.InQuad));

                // 4. Hit target
                seq.AppendCallback(() =>
                {
                    if (!hasPopped && targetSlot != null)
                    {
                        hasPopped = true;
                        targetSlot.PlayPop();
                        targetSlot.AnimateCountUp(oldAmount, newAmount, countUpDuration);
                    }
                });

                // 5. Shrink & disappear into slot icon
                seq.Append(pObj.transform.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InQuad));

                seq.OnComplete(() =>
                {
                    landedCount++;
                    _activeParticles.Remove(pObj);
                    _activeSequences.Remove(seq);
                    if (pObj != null)
                    {
                        Destroy(pObj);
                    }

                    if (landedCount >= count)
                    {
                        // Allow count-up text to finish nicely before proceeding
                        float waitExtra = Mathf.Max(0.08f, countUpDuration - 0.15f);
                        DOVirtual.DelayedCall(waitExtra, () =>
                        {
                            onComplete?.Invoke();
                        });
                    }
                });
            }
        }

        private GameObject CreateParticleObject(Sprite rewardIcon, Sprite frameSprite, Transform parent)
        {
            GameObject obj = new GameObject("ui_particle_reward", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = particleSize;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image frameImg = obj.GetComponent<Image>();
            frameImg.sprite = frameSprite;
            frameImg.type = frameSprite != null && frameSprite.border != Vector4.zero 
                ? Image.Type.Sliced 
                : Image.Type.Simple;
            frameImg.preserveAspect = false;
            frameImg.raycastTarget = false;

            // Child reward icon
            GameObject iconObj = new GameObject("ui_icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(obj.transform, false);

            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.sizeDelta = iconSize;
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;

            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = rewardIcon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            return obj;
        }

        public void ClearActiveParticles()
        {
            foreach (var seq in _activeSequences)
            {
                seq?.Kill();
            }
            _activeSequences.Clear();

            foreach (var p in _activeParticles)
            {
                if (p != null) Destroy(p);
            }
            _activeParticles.Clear();
        }

        private void OnDestroy()
        {
            ClearActiveParticles();
        }
    }
}

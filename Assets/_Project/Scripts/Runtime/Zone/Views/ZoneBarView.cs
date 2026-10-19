using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Events;
using Vertigo.Zone.Data;

namespace Vertigo.Zone.Views
{
    public class ZoneBarView : MonoBehaviour
    {
        private static readonly Color CurrentNormalTextColor = new Color(0.08f, 0.08f, 0.10f, 1f);

        [Header("Zone Strip Components")]
        [Tooltip("The parent container holding zone number items")]
        [SerializeField] private RectTransform ui_container_zone_slots;
        [SerializeField] private GameObject ui_prefab_zone_slot;

        [Header("Slot References")]
        [SerializeField] private List<TMP_Text> ui_slot_texts = new List<TMP_Text>();
        [SerializeField] private List<Image> ui_slot_backgrounds = new List<Image>();

        [Header("Current Zone Sliding Badge")]
        [SerializeField] private RectTransform ui_transform_current_badge;
        [SerializeField] private Image ui_image_current_badge;
        [SerializeField] private TMP_Text ui_text_current_zone_value;

        [Header("Badge Sprites")]
        [SerializeField] private Sprite badgeNormalSprite;
        [SerializeField] private Sprite badgeSafeSprite;
        [SerializeField] private Sprite badgeSuperSprite;

        [Header("Top Right Badges")]
        [SerializeField] private TMP_Text ui_text_next_super_zone_value;
        [SerializeField] private TMP_Text ui_text_next_safe_zone_value;
        [SerializeField] private GameObject ui_badge_safe_zone;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color safeColor = new Color(0.35f, 0.95f, 0.15f, 1f);
        [SerializeField] private Color superColor = new Color(1.00f, 0.84f, 0.00f, 1f);
        [SerializeField] private Color pastZoneColor = new Color(0.42f, 0.44f, 0.48f, 1f);
        [SerializeField] private Color pastSafeColor = new Color(0.20f, 0.42f, 0.12f, 1f);
        [SerializeField] private Color pastSuperColor = new Color(0.55f, 0.42f, 0.12f, 1f);

        [Header("Smooth DOTween Animation Settings")]
        [SerializeField] private float slideDuration = 0.35f;
        [SerializeField] private Ease slideEase = Ease.OutCubic;
        [SerializeField] private float punchScaleAmount = 0.08f;
        [SerializeField] private int maxZone = 60;

        private int _currentZoneNumber;
        private Sequence _slideSequence;
        private RectTransform _rootRectTransform;

        private int ActiveZone => _currentZoneNumber > 0 ? _currentZoneNumber : 1;
        private int TotalSlots => ui_slot_texts is { Count: > 0 } ? ui_slot_texts.Count : 13;
        private int HalfSlots => TotalSlots / 2;

        private HorizontalLayoutGroup _hlg;

        private void Awake()
        {
            _rootRectTransform = (RectTransform)transform;
            if (ui_container_zone_slots != null)
                _hlg = ui_container_zone_slots.GetComponent<HorizontalLayoutGroup>();
        }

        private IEnumerator Start()
        {
            SnapToZone(ActiveZone);
            yield return null;
            SnapToZone(ActiveZone);
        }

        private void OnEnable()
        {
            GameEvents.OnZoneChanged += UpdateZoneBar;
            SnapToZone(ActiveZone);
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged -= UpdateZoneBar;
            KillSequence();
        }

        private void OnDestroy() => KillSequence();

        private void KillSequence()
        {
            if (_slideSequence != null && _slideSequence.IsActive())
                _slideSequence.Kill();

            if (ui_container_zone_slots != null)
            {
                ui_container_zone_slots.DOKill();
                ui_container_zone_slots.anchoredPosition = Vector2.zero;
            }
        }

        public void UpdateZoneBar(ZoneData zoneData)
        {
            if (zoneData == null) return;
            int newZone = Mathf.Clamp(zoneData.ZoneNumber, 1, maxZone);

            UpdateNextBadges(newZone);

            bool shouldAnimate = _currentZoneNumber > 0
                && Application.isPlaying
                && ui_transform_current_badge != null
                && newZone == _currentZoneNumber + 1;

            if (shouldAnimate)
                AnimateToZone(newZone);
            else
                SnapToZone(newZone);
        }

        private void UpdateNextBadges(int zoneNumber)
        {
            int nextSafe = Mathf.Min(maxZone, ((zoneNumber / 5) + 1) * 5);
            if (IsSuperZone(nextSafe))
                nextSafe = Mathf.Min(maxZone, nextSafe + 5);

            int nextSuper = Mathf.Min(maxZone, ((zoneNumber / 30) + 1) * 30);

            if (ui_text_next_safe_zone_value != null)
                ui_text_next_safe_zone_value.text = nextSafe.ToString();

            if (ui_text_next_super_zone_value != null)
                ui_text_next_super_zone_value.text = nextSuper.ToString();

            UpdateSafeZoneBadgeVisibility(zoneNumber);
        }

        private void UpdateSafeZoneBadgeVisibility(int zoneNumber)
        {
            if (ui_badge_safe_zone == null && ui_text_next_safe_zone_value != null)
                ui_badge_safe_zone = ui_text_next_safe_zone_value.transform.parent.gameObject;

            if (ui_badge_safe_zone == null) return;

            bool showSafeBadge = zoneNumber < 55;
            if (ui_badge_safe_zone.activeSelf != showSafeBadge)
                ui_badge_safe_zone.SetActive(showSafeBadge);
        }

        private int CalculateStartZone(int zone)
        {
            int maxStartZone = Mathf.Max(1, maxZone - TotalSlots + 1);
            return Mathf.Clamp(zone - HalfSlots, 1, maxStartZone);
        }

        public void SnapToZone(int currentZone)
        {
            KillSequence();
            currentZone = Mathf.Clamp(currentZone, 1, maxZone);
            _currentZoneNumber = currentZone;

            UpdateNextBadges(currentZone);

            int startZone = CalculateStartZone(currentZone);
            ApplyZoneVisuals(currentZone, startZone);

            if (ui_transform_current_badge != null)
            {
                ui_transform_current_badge.DOKill();
                ui_transform_current_badge.localScale = Vector3.one;
                int targetIndex = Mathf.Clamp(currentZone - startZone, 0, TotalSlots - 1);
                ui_transform_current_badge.anchoredPosition = new Vector2(GetSlotLocalX(targetIndex), 0f);
            }
        }

        private void AnimateToZone(int newZone)
        {
            KillSequence();
            newZone = Mathf.Clamp(newZone, 1, maxZone);
            _slideSequence = DOTween.Sequence();

            int previousZone = _currentZoneNumber;
            int prevStartZone = CalculateStartZone(previousZone);
            int nextStartZone = CalculateStartZone(newZone);
            SetSlotPastColor(previousZone, prevStartZone);

            if (nextStartZone == prevStartZone)
            {
                int targetIndex = Mathf.Clamp(newZone - nextStartZone, 0, TotalSlots - 1);
                float targetX = GetSlotLocalX(targetIndex);

                _slideSequence.Append(ui_transform_current_badge.DOAnchorPosX(targetX, slideDuration).SetEase(slideEase));
            }
            else
            {
                ui_transform_current_badge.anchoredPosition = new Vector2(GetSlotLocalX(HalfSlots), 0f);

                _slideSequence.Append(ui_container_zone_slots.DOAnchorPosX(-GetSlotStepDistance(), slideDuration).SetEase(slideEase));
                if (ui_slot_texts is { Count: > 0 } && ui_slot_texts[0] != null)
                    _slideSequence.Join(ui_slot_texts[0].DOFade(0f, slideDuration * 0.8f));
            }

            _slideSequence.AppendCallback(() => CompleteZoneAnimation(newZone, nextStartZone));
        }

        private void CompleteZoneAnimation(int newZone, int startZone)
        {
            if (ui_container_zone_slots != null)
                ui_container_zone_slots.anchoredPosition = Vector2.zero;

            _currentZoneNumber = newZone;
            ApplyZoneVisuals(newZone, startZone);
            ui_transform_current_badge.DOPunchScale(Vector3.one * punchScaleAmount, 0.2f, 2, 0.5f);
        }

        private void ApplyZoneVisuals(int zone, int startZone)
        {
            RefreshSlotNumbers(startZone);
            RefreshSlotColors(zone, startZone);
            UpdateBadgeVisual(zone);
        }

        private void SetSlotPastColor(int zoneNumber, int startZone)
        {
            int index = zoneNumber - startZone;
            if (ui_slot_texts == null || index < 0 || index >= ui_slot_texts.Count || ui_slot_texts[index] == null) return;

            (ui_slot_texts[index].color, ui_slot_texts[index].fontStyle) = GetZoneStyle(zoneNumber, isPast: true);
        }

        private static bool IsSuperZone(int zoneNumber) => zoneNumber > 0 && zoneNumber % 30 == 0;

        public bool IsSafeZone(int zoneNumber) =>
            zoneNumber > 0 && zoneNumber <= maxZone && !IsSuperZone(zoneNumber) && (zoneNumber == 1 || zoneNumber % 5 == 0);

        public void UpdateBadgeVisual(int zone)
        {
            if (ui_image_current_badge == null) return;

            Sprite targetSprite = IsSuperZone(zone) ? badgeSuperSprite
                : IsSafeZone(zone) ? badgeSafeSprite
                : badgeNormalSprite;

            if (targetSprite != null)
                ui_image_current_badge.sprite = targetSprite;
        }

        private void RefreshSlotNumbers(int startZone)
        {
            if (ui_slot_texts == null) return;
            for (int i = 0; i < ui_slot_texts.Count; i++)
            {
                if (ui_slot_texts[i] != null)
                    ui_slot_texts[i].text = (startZone + i).ToString();
            }
        }

        private void RefreshSlotColors(int currentZone, int startZone)
        {
            if (ui_slot_texts == null) return;

            for (int i = 0; i < ui_slot_texts.Count; i++)
            {
                TMP_Text slotTxt = ui_slot_texts[i];
                if (slotTxt == null) continue;

                int zoneNum = startZone + i;
                if (zoneNum == currentZone)
                {
                    slotTxt.color = IsSuperZone(currentZone) || IsSafeZone(currentZone) ? Color.white : CurrentNormalTextColor;
                    slotTxt.fontStyle = FontStyles.Bold;
                }
                else
                {
                    (slotTxt.color, slotTxt.fontStyle) = GetZoneStyle(zoneNum, isPast: zoneNum < currentZone);
                }
            }
        }

        private (Color color, FontStyles style) GetZoneStyle(int zoneNum, bool isPast)
        {
            if (IsSuperZone(zoneNum)) return (isPast ? pastSuperColor : superColor, FontStyles.Bold);
            if (IsSafeZone(zoneNum)) return (isPast ? pastSafeColor : safeColor, FontStyles.Bold);
            return (isPast ? pastZoneColor : normalColor, FontStyles.Normal);
        }

        private bool TryGetStripMetrics(out float containerW, out float padLeft, out float slotW, out float spacing)
        {
            if (ui_container_zone_slots == null)
            {
                containerW = padLeft = slotW = spacing = 0f;
                return false;
            }

            containerW = ui_container_zone_slots.rect.width > 0f ? ui_container_zone_slots.rect.width : 725f;
            padLeft = _hlg != null ? _hlg.padding.left : 0f;
            float padRight = _hlg != null ? _hlg.padding.right : 0f;
            spacing = _hlg != null ? _hlg.spacing : 4.5f;

            int count = TotalSlots;
            float availW = containerW - (padLeft + padRight);
            slotW = (availW - (count - 1) * spacing) / count;
            return true;
        }

        private float GetSlotStepDistance() =>
            TryGetStripMetrics(out _, out _, out float slotW, out float spacing) ? slotW + spacing : 56.1f;

        private float GetSlotLocalX(int index)
        {
            if (ui_slot_texts == null || index < 0 || index >= ui_slot_texts.Count) return 0f;

            if (TryGetStripMetrics(out float containerW, out float padLeft, out float slotW, out float spacing))
            {
                float centerFromLeft = padLeft + index * (slotW + spacing) + (slotW * 0.5f);
                return centerFromLeft - (containerW * 0.5f) + ui_container_zone_slots.anchoredPosition.x;
            }

            if (_rootRectTransform == null)
                _rootRectTransform = (RectTransform)transform;

            RectTransform slotRT = ui_slot_texts[index].rectTransform.parent as RectTransform ?? ui_slot_texts[index].rectTransform;
            return _rootRectTransform.InverseTransformPoint(slotRT.position).x;
        }
    }
}

using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Zone.Data;

namespace Vertigo.Zone.Views
{
    public class ZoneBarView : MonoBehaviour
    {
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

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f); // Crisp white
        [SerializeField] private Color safeColor = new Color(0.35f, 0.95f, 0.15f, 1f); // Vivid neon lime #59F226
        [SerializeField] private Color superColor = new Color(1.00f, 0.84f, 0.00f, 1f); // Bright gold #FFD700
        [SerializeField] private Color pastZoneColor = new Color(0.42f, 0.44f, 0.48f, 1f); // Muted grey #6B707B
        [SerializeField] private Color pastSafeColor = new Color(0.20f, 0.42f, 0.12f, 1f); // Muted dark olive green #336B1E
        [SerializeField] private Color pastSuperColor = new Color(0.55f, 0.42f, 0.12f, 1f); // Muted dark gold

        [Header("Smooth DOTween Animation Settings")]
        [SerializeField] private float slideDuration = 0.35f;
        [SerializeField] private Ease slideEase = Ease.OutCubic;
        [SerializeField] private float punchScaleAmount = 0.08f;

        private int _currentZoneNumber = 0;
        private Sequence _slideSequence;
        private RectTransform _rootRectTransform;

        private void Awake()
        {
            _rootRectTransform = GetComponent<RectTransform>();
        }

        private System.Collections.IEnumerator Start()
        {
            // Frame 0: Snap immediately
            int zone = _currentZoneNumber > 0 ? _currentZoneNumber : 1;
            SnapToZone(zone);

            // Wait 1 frame for CanvasScaler and HorizontalLayoutGroup to fully evaluate
            yield return null;

            SnapToZone(_currentZoneNumber > 0 ? _currentZoneNumber : 1);
        }

        private void OnEnable()
        {
            GameEvents.OnZoneChanged += UpdateZoneBar;
            SnapToZone(_currentZoneNumber > 0 ? _currentZoneNumber : 1);
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged -= UpdateZoneBar;
            KillSequence();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        private void KillSequence()
        {
            if (_slideSequence != null && _slideSequence.IsActive())
            {
                _slideSequence.Kill();
            }
        }

        public void UpdateZoneBar(ZoneData zoneData)
        {
            if (zoneData == null) return;
            int newZone = zoneData.ZoneNumber;

            // 1. Update Top-Right Next Badges
            int nextSafe = ((newZone / 5) + (newZone % 5 == 0 ? 0 : 1)) * 5;
            if (nextSafe == newZone && newZone % 5 == 0)
            {
                nextSafe = newZone;
            }
            int nextSuper = ((newZone / 30) + (newZone % 30 == 0 ? 0 : 1)) * 30;

            if (ui_text_next_safe_zone_value != null)
                ui_text_next_safe_zone_value.text = nextSafe.ToString();

            if (ui_text_next_super_zone_value != null)
                ui_text_next_super_zone_value.text = nextSuper.ToString();

            // 2. Initial Setup or Edit Mode: Snap immediately without tweening
            if (_currentZoneNumber == 0 || !Application.isPlaying || ui_transform_current_badge == null)
            {
                _currentZoneNumber = newZone;
                SnapToZone(newZone);
                return;
            }

            // 3. Same Zone: Just refresh
            if (newZone == _currentZoneNumber)
            {
                SnapToZone(newZone);
                return;
            }

            // 4. Animate with smooth DOTween slide!
            AnimateToZone(newZone);
        }

        public void SnapToZone(int currentZone)
        {
            KillSequence();
            _currentZoneNumber = currentZone;

            int totalSlots = ui_slot_texts != null && ui_slot_texts.Count > 0 ? ui_slot_texts.Count : 13;
            int halfSlots = totalSlots / 2;
            int startZone = Mathf.Max(1, currentZone - halfSlots);

            RefreshSlotNumbers(startZone);
            RefreshSlotColors(currentZone, startZone);
            UpdateBadgeVisual(currentZone);

            if (ui_transform_current_badge != null)
            {
                int targetIndex = Mathf.Clamp(currentZone - startZone, 0, totalSlots - 1);
                float targetX = GetSlotLocalX(targetIndex);
                ui_transform_current_badge.anchoredPosition = new Vector2(targetX, 0);
            }
        }

        private void AnimateToZone(int newZone)
        {
            KillSequence();
            _slideSequence = DOTween.Sequence();

            int totalSlots = ui_slot_texts != null && ui_slot_texts.Count > 0 ? ui_slot_texts.Count : 13;
            int halfSlots = totalSlots / 2; // e.g. 6 in 13 slots
            int previousZone = _currentZoneNumber;

            if (newZone < _currentZoneNumber)
            {
                // Reset to beginning (e.g. restart from Zone 1)
                int startZone = 1;
                RefreshSlotNumbers(startZone);
                RefreshSlotColors(newZone, startZone);
                UpdateBadgeVisual(newZone);

                float targetX = GetSlotLocalX(0);

                _slideSequence.Append(ui_transform_current_badge.DOAnchorPosX(targetX, slideDuration * 1.2f).SetEase(Ease.InOutCubic));
                _slideSequence.AppendCallback(() =>
                {
                    _currentZoneNumber = newZone;
                    ui_transform_current_badge.DOPunchScale(Vector3.one * punchScaleAmount, 0.2f, 2, 0.5f);
                });
                return;
            }

            // Advancing forward
            int prevStartZone = Mathf.Max(1, previousZone - halfSlots);
            SetSlotPastColor(previousZone, prevStartZone);

            if (newZone <= halfSlots + 1)
            {
                // Case A: Early Progression (Zones 1..7)
                // Badge slides smoothly from current slot to target slot along the strip
                int targetIndex = newZone - 1;
                float targetX = GetSlotLocalX(targetIndex);

                _slideSequence.Append(ui_transform_current_badge.DOAnchorPosX(targetX, slideDuration).SetEase(slideEase));
                _slideSequence.AppendCallback(() =>
                {
                    _currentZoneNumber = newZone;
                    UpdateBadgeVisual(newZone);
                    RefreshSlotColors(newZone, 1);
                    ui_transform_current_badge.DOPunchScale(Vector3.one * punchScaleAmount, 0.2f, 2, 0.5f);
                });
            }
            else
            {
                // Case B: Centered Progression (Zones > 7)
                // The new number is currently at slot (halfSlots + 1)
                int nextIndex = halfSlots + 1;
                float nextX = GetSlotLocalX(nextIndex);
                float centerX = GetSlotLocalX(halfSlots);

                // 1. Badge slides right onto the incoming number!
                _slideSequence.Append(ui_transform_current_badge.DOAnchorPosX(nextX, slideDuration * 0.75f).SetEase(slideEase));
                _slideSequence.AppendCallback(() =>
                {
                    // 2. Tape centers the new zone under the badge seamlessly
                    _currentZoneNumber = newZone;
                    UpdateBadgeVisual(newZone);
                    int startZone = newZone - halfSlots;
                    RefreshSlotNumbers(startZone);
                    RefreshSlotColors(newZone, startZone);

                    ui_transform_current_badge.anchoredPosition = new Vector2(centerX, 0);
                    ui_transform_current_badge.DOPunchScale(Vector3.one * punchScaleAmount, 0.2f, 2, 0.5f);
                });
            }
        }

        private void SetSlotPastColor(int zoneNumber, int startZone)
        {
            if (ui_slot_texts == null) return;
            int index = zoneNumber - startZone;
            if (index >= 0 && index < ui_slot_texts.Count)
            {
                TMP_Text txt = ui_slot_texts[index];
                if (txt != null)
                {
                    if (zoneNumber % 30 == 0)
                    {
                        txt.color = pastSuperColor;
                        txt.fontStyle = FontStyles.Bold;
                    }
                    else if (zoneNumber % 5 == 0)
                    {
                        txt.color = pastSafeColor;
                        txt.fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        txt.color = pastZoneColor;
                        txt.fontStyle = FontStyles.Normal;
                    }
                }
            }
        }

        public void UpdateBadgeVisual(int zone)
        {
            if (ui_image_current_badge == null && ui_transform_current_badge != null)
            {
                ui_image_current_badge = ui_transform_current_badge.GetComponent<Image>();
            }

            if (ui_image_current_badge == null) return;

            if (zone % 30 == 0 && badgeSuperSprite != null)
            {
                ui_image_current_badge.sprite = badgeSuperSprite;
            }
            else if (zone % 5 == 0 && badgeSafeSprite != null)
            {
                ui_image_current_badge.sprite = badgeSafeSprite;
            }
            else if (badgeNormalSprite != null)
            {
                ui_image_current_badge.sprite = badgeNormalSprite;
            }
        }

        private void RefreshSlotNumbers(int startZone)
        {
            if (ui_slot_texts == null) return;
            for (int i = 0; i < ui_slot_texts.Count; i++)
            {
                if (ui_slot_texts[i] != null)
                {
                    ui_slot_texts[i].text = (startZone + i).ToString();
                }
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
                    // Current zone: bold white on safe / super badges, bold black on normal silver badge
                    if (currentZone % 30 == 0 || currentZone % 5 == 0)
                    {
                        slotTxt.color = Color.white;
                    }
                    else
                    {
                        slotTxt.color = new Color(0.08f, 0.08f, 0.10f, 1f);
                    }
                    slotTxt.fontStyle = FontStyles.Bold;
                }
                else if (zoneNum < currentZone)
                {
                    // Past zones
                    if (zoneNum % 30 == 0)
                    {
                        slotTxt.color = pastSuperColor;
                        slotTxt.fontStyle = FontStyles.Bold;
                    }
                    else if (zoneNum % 5 == 0)
                    {
                        slotTxt.color = pastSafeColor;
                        slotTxt.fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        slotTxt.color = pastZoneColor;
                        slotTxt.fontStyle = FontStyles.Normal;
                    }
                }
                else
                {
                    // Upcoming zones
                    if (zoneNum % 30 == 0)
                    {
                        slotTxt.color = superColor;
                        slotTxt.fontStyle = FontStyles.Bold;
                    }
                    else if (zoneNum % 5 == 0)
                    {
                        slotTxt.color = safeColor;
                        slotTxt.fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        slotTxt.color = normalColor;
                        slotTxt.fontStyle = FontStyles.Normal;
                    }
                }
            }
        }

        private float GetSlotLocalX(int index)
        {
            if (ui_slot_texts == null || index < 0 || index >= ui_slot_texts.Count) return 0f;

            // 1. Deterministic mathematical calculation from container width & HorizontalLayoutGroup
            if (ui_container_zone_slots != null)
            {
                float containerW = ui_container_zone_slots.rect.width;
                if (containerW <= 0f) containerW = 725f;

                HorizontalLayoutGroup hlg = ui_container_zone_slots.GetComponent<HorizontalLayoutGroup>();
                float padLeft = hlg != null ? hlg.padding.left : 0f;
                float padRight = hlg != null ? hlg.padding.right : 0f;
                float spacing = hlg != null ? hlg.spacing : 4.5f;

                int count = ui_slot_texts.Count;
                float availW = containerW - (padLeft + padRight);
                float totalSpacing = (count - 1) * spacing;
                float slotW = count > 0 ? (availW - totalSpacing) / count : 51f;

                float centerFromLeft = padLeft + index * (slotW + spacing) + (slotW * 0.5f);
                float localX = centerFromLeft - (containerW * 0.5f);
                return localX + ui_container_zone_slots.anchoredPosition.x;
            }

            // 2. Fallback to transform hierarchy
            if (_rootRectTransform == null)
            {
                _rootRectTransform = GetComponent<RectTransform>();
            }

            RectTransform slotRT = ui_slot_texts[index].rectTransform.parent as RectTransform;
            if (slotRT == null)
            {
                slotRT = ui_slot_texts[index].rectTransform;
            }

            Vector3 worldPos = slotRT.position;
            Vector3 localPos = _rootRectTransform.InverseTransformPoint(worldPos);
            return localPos.x;
        }
    }
}

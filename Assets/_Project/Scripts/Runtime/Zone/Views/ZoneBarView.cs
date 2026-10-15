using System.Collections.Generic;
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
        [Header("Zone Strip Slots")]
        [Tooltip("The parent container holding zone number items")]
        [SerializeField] private Transform ui_container_zone_slots;
        [SerializeField] private GameObject ui_prefab_zone_slot;

        [Header("Slot References (Pre-instantiated slots)")]
        [SerializeField] private List<TMP_Text> ui_slot_texts = new List<TMP_Text>();
        [SerializeField] private List<Image> ui_slot_backgrounds = new List<Image>();

        [Header("Current Zone Badge")]
        [SerializeField] private RectTransform ui_transform_current_badge;
        [SerializeField] private TMP_Text ui_text_current_zone_value;

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

        private void OnEnable()
        {
            GameEvents.OnZoneChanged += UpdateZoneBar;
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged -= UpdateZoneBar;
        }

        public void UpdateZoneBar(ZoneData zoneData)
        {
            if (zoneData == null) return;
            int currentZone = zoneData.ZoneNumber;

            // 1. Update Top-Right Next Badges
            int nextSafe = ((currentZone / 5) + (currentZone % 5 == 0 ? 0 : 1)) * 5;
            if (nextSafe == currentZone && currentZone % 5 == 0)
            {
                // Already at safe zone, point to current or next
                nextSafe = currentZone;
            }
            int nextSuper = ((currentZone / 30) + (currentZone % 30 == 0 ? 0 : 1)) * 30;

            if (ui_text_next_safe_zone_value != null)
                ui_text_next_safe_zone_value.text = nextSafe.ToString();

            if (ui_text_next_super_zone_value != null)
                ui_text_next_super_zone_value.text = nextSuper.ToString();

            // 2. Update Horizontal Zone Strip Numbers
            int totalSlots = ui_slot_texts != null && ui_slot_texts.Count > 0 ? ui_slot_texts.Count : 13;
            int halfSlots = totalSlots / 2;

            // Calculate start zone so currentZone is centered
            int startZone = Mathf.Max(1, currentZone - halfSlots);

            if (ui_slot_texts != null && ui_slot_texts.Count > 0)
            {
                for (int i = 0; i < ui_slot_texts.Count; i++)
                {
                    int zoneNum = startZone + i;
                    TMP_Text slotTxt = ui_slot_texts[i];
                    if (slotTxt == null) continue;

                    slotTxt.text = zoneNum.ToString();

                    // Formatting
                    if (zoneNum == currentZone)
                    {
                        // Current zone text inside the silver badge
                        slotTxt.color = new Color(0.08f, 0.08f, 0.10f, 1f);
                        slotTxt.fontStyle = FontStyles.Bold;

                        if (ui_slot_backgrounds != null && i < ui_slot_backgrounds.Count && ui_slot_backgrounds[i] != null)
                        {
                            ui_slot_backgrounds[i].enabled = true;
                        }
                    }
                    else
                    {
                        if (ui_slot_backgrounds != null && i < ui_slot_backgrounds.Count && ui_slot_backgrounds[i] != null)
                        {
                            ui_slot_backgrounds[i].enabled = false;
                        }

                        if (zoneNum < currentZone)
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
            }
        }
    }
}

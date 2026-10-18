using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core.Enums;
using Vertigo.Core.Events;
using Vertigo.Core.Interfaces;
using Vertigo.Wheel.Controllers;
using Vertigo.Wheel.Data;
using Vertigo.Zone.Data;

namespace Vertigo.UI.Testing
{
    public class GameTestHarnessUI : MonoBehaviour
    {
        [Header("Wheel UI Elements")]
        [SerializeField] private RectTransform ui_transform_wheel_rotator;
        [SerializeField] private Image ui_image_wheel_base;
        [SerializeField] private Image ui_image_wheel_indicator;
        [SerializeField] private Button ui_button_spin;

        [Header("Zone Bar Elements")]
        [SerializeField] private TMP_Text ui_text_zone_value;
        [SerializeField] private TMP_Text ui_text_zone_type_value;
        [SerializeField] private TMP_Text ui_text_multiplier_value;

        [Header("Loot & Cashout Elements")]
        [SerializeField] private Button ui_button_cashout;
        [SerializeField] private TMP_Text ui_text_cashout_status_value;
        [SerializeField] private TMP_Text ui_text_loot_list_value;

        [Header("Status & Bomb Popup")]
        [SerializeField] private TMP_Text ui_text_game_status_value;
        [SerializeField] private GameObject ui_panel_bomb_popup;
        [SerializeField] private Button ui_button_giveup;
        [SerializeField] private Button ui_button_revive;

        [Header("Dependencies")]
        [SerializeField] private WheelController wheelController;

        private Tween _spinTween;

        private void Awake()
        {
            // Connect button clicks in code (strict requirement: No UnityEvents in Inspector)
            if (ui_button_spin != null)
                ui_button_spin.onClick.AddListener(OnSpinClicked);

            if (ui_button_cashout != null)
                ui_button_cashout.onClick.AddListener(OnCashOutClicked);

            if (ui_button_giveup != null)
                ui_button_giveup.onClick.AddListener(OnGiveUpClicked);

            if (ui_button_revive != null)
                ui_button_revive.onClick.AddListener(OnReviveClicked);

            if (ui_panel_bomb_popup != null)
                ui_panel_bomb_popup.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            GameEvents.OnZoneChanged += OnZoneChanged;
            GameEvents.OnSpinStarted += OnSpinStarted;
            GameEvents.OnSpinCompleted += OnSpinCompleted;
            GameEvents.OnInventoryUpdated += OnInventoryUpdated;
            GameEvents.OnBombTriggered += OnBombTriggered;
            GameEvents.OnReviveSuccess += OnReviveSuccess;
            GameEvents.OnCashOutSuccess += OnCashOutSuccess;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.OnZoneChanged -= OnZoneChanged;
            GameEvents.OnSpinStarted -= OnSpinStarted;
            GameEvents.OnSpinCompleted -= OnSpinCompleted;
            GameEvents.OnInventoryUpdated -= OnInventoryUpdated;
            GameEvents.OnBombTriggered -= OnBombTriggered;
            GameEvents.OnReviveSuccess -= OnReviveSuccess;
            GameEvents.OnCashOutSuccess -= OnCashOutSuccess;
            _spinTween?.Kill();
        }

        private void OnSpinClicked()
        {
            GameEvents.FireSpinRequested();
        }

        private void OnCashOutClicked()
        {
            GameEvents.FireCashOutRequested();
        }

        private void OnGiveUpClicked()
        {
            if (ui_panel_bomb_popup != null)
                ui_panel_bomb_popup.SetActive(false);
            GameEvents.FireGameRestartRequested();
        }

        private void OnReviveClicked()
        {
            if (ui_panel_bomb_popup != null)
                ui_panel_bomb_popup.SetActive(false);
            GameEvents.FireReviveRequested();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (ui_text_game_status_value != null)
                ui_text_game_status_value.text = $"Status: {state}";

            bool isIdle = state == GameState.Idle;
            if (ui_button_spin != null)
                ui_button_spin.interactable = isIdle;
        }

        private void OnZoneChanged(ZoneData zone)
        {
            if (zone == null) return;

            if (ui_text_zone_value != null)
                ui_text_zone_value.text = $"ZONE {zone.ZoneNumber}";

            if (ui_text_zone_type_value != null)
            {
                string typeLabel = zone.ZoneType switch
                {
                    ZoneType.Safe => "<color=#4CD964>SAFE ZONE (Silver Spin)</color>",
                    ZoneType.Super => "<color=#FFD700>SUPER ZONE (Golden Spin)</color>",
                    _ => "<color=#E5E5E5>NORMAL ZONE</color>"
                };
                ui_text_zone_type_value.text = typeLabel;
            }

            if (ui_text_multiplier_value != null)
                ui_text_multiplier_value.text = $"Reward Multiplier: x{zone.RewardMultiplier:F2}";

            // Update wheel sprites
            if (zone.WheelConfig != null)
            {
                if (ui_image_wheel_base != null && zone.WheelConfig.WheelBaseSprite != null)
                    ui_image_wheel_base.sprite = zone.WheelConfig.WheelBaseSprite;

                if (ui_image_wheel_indicator != null && zone.WheelConfig.IndicatorSprite != null)
                    ui_image_wheel_indicator.sprite = zone.WheelConfig.IndicatorSprite;
            }

            // Cash out permission: available in safe & super zones when idle
            bool canCashOut = zone.CanCashOut;
            if (ui_button_cashout != null)
                ui_button_cashout.interactable = canCashOut;

            if (ui_text_cashout_status_value != null)
            {
                ui_text_cashout_status_value.text = canCashOut
                    ? "<color=#4CD964>Cash Out Available</color>"
                    : "<color=#888888>Only in Safe/Super Zone</color>";
            }
        }

        private void OnSpinStarted(int targetIndex, float targetAngle, float exactCenterAngle, float duration)
        {
            if (ui_button_cashout != null)
                ui_button_cashout.interactable = false;

            if (ui_transform_wheel_rotator != null)
            {
                _spinTween?.Kill();
                // Animate wheel rotation smoothly using DOTween
                _spinTween = ui_transform_wheel_rotator.DORotate(new Vector3(0, 0, targetAngle), duration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        wheelController?.OnSpinAnimationComplete();
                    });
            }
            else
            {
                // Fallback if no rotator assigned
                DOVirtual.DelayedCall(duration, () => wheelController?.OnSpinAnimationComplete());
            }
        }

        private void OnSpinCompleted(WheelSliceData slice)
        {
            if (slice?.Reward == null) return;

            string sliceInfo = slice.Reward.IsBomb 
                ? "<color=#FF3B30>BOMB EXPLODED!</color>" 
                : $"<color=#FFCC00>+ {slice.Reward.DisplayName} (x{slice.Amount})</color>";

            if (ui_text_game_status_value != null)
                ui_text_game_status_value.text = $"Result: {sliceInfo}";
        }

        private void OnInventoryUpdated(IReadOnlyDictionary<string, (IRewardData reward, int amount)> items)
        {
            if (ui_text_loot_list_value == null) return;

            if (items == null || items.Count == 0)
            {
                ui_text_loot_list_value.text = "<color=#666666>No rewards collected yet</color>";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in items)
            {
                sb.AppendLine($"• {kvp.Value.reward.DisplayName}: <b>{kvp.Value.reward.FormatAmount(kvp.Value.amount)}</b>");
            }
            ui_text_loot_list_value.text = sb.ToString();
        }

        private void OnBombTriggered()
        {
            if (ui_panel_bomb_popup != null)
                ui_panel_bomb_popup.SetActive(true);
        }

        private void OnReviveSuccess()
        {
            if (ui_panel_bomb_popup != null)
                ui_panel_bomb_popup.SetActive(false);
        }

        private void OnCashOutSuccess(IReadOnlyDictionary<string, (IRewardData reward, int amount)> banked)
        {
            if (ui_text_game_status_value != null)
                ui_text_game_status_value.text = $"<color=#4CD964>Congratulations! {banked.Count} kinds of rewards banked!</color>";
        }

        private void OnValidate()
        {
            // Strict doc rule: Button references should be automatically set from OnValidate
            if (ui_button_spin == null)
            {
                var tr = transform.Find("ui_button_spin");
                if (tr != null) ui_button_spin = tr.GetComponent<Button>();
            }
            if (ui_button_cashout == null)
            {
                var tr = transform.Find("ui_panel_left/ui_button_cashout");
                if (tr != null) ui_button_cashout = tr.GetComponent<Button>();
            }
            if (wheelController == null)
            {
                wheelController = FindFirstObjectByType<WheelController>();
            }
        }
    }
}

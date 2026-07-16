using System;
using UnityEngine;
using UnityEngine.UI;

namespace AstroScope
{
	/// <summary>
	/// Builds a runtime UGUI control bar that displays the simulated clock and lets the user
	/// jump through time, change simulation speed, and pause the clock.
	/// </summary>
	public class AstroTimeControlPanel : MonoBehaviour
	{
		private const float PanelHeight = 96f;
		private const float ButtonMinWidth = 104f;
		private const float TimeLabelMinWidth = 460f;
		private const int ButtonFontSize = 24;
		private const int TimeFontSize = 26;

		private static readonly float[] SpeedSteps = { 1f, 60f, 3600f, 86400f, 604800f };

		/// <summary>Clock controller driven by this panel. Assigned by the scene builder.</summary>
		public AstroClockController clock;

		/// <summary>Canvas that hosts the panel. Assigned by the scene builder.</summary>
		public Canvas canvas;

		private Text _timeText;
		private Text _speedButtonLabel;
		private Text _pauseButtonLabel;
		private int _speedIndex;

		private void Start()
		{
			if (clock == null)
			{
				clock = FindFirstObjectByType<AstroClockController>();
			}

			if (clock == null)
			{
				Debug.LogWarning("[AstroScope] AstroTimeControlPanel could not find an AstroClockController.");
				enabled = false;
				return;
			}

			if (canvas == null)
			{
				canvas = GetComponentInParent<Canvas>();
			}

			if (canvas == null)
			{
				Debug.LogWarning("[AstroScope] AstroTimeControlPanel could not find a Canvas.");
				enabled = false;
				return;
			}

			BuildPanel();
		}

		private void Update()
		{
			if (_timeText == null || clock == null)
			{
				return;
			}

			DateTime local = clock.CurrentLocal;
			_timeText.text = $"{local:yyyy-MM-dd (ddd) HH:mm:ss}";
		}

		private void BuildPanel()
		{
			var panelObject = new GameObject("TimeControlPanel", typeof(RectTransform));
			var panelRect = (RectTransform)panelObject.transform;
			panelRect.SetParent(canvas.transform, false);
			panelRect.anchorMin = new Vector2(0f, 0f);
			panelRect.anchorMax = new Vector2(1f, 0f);
			panelRect.pivot = new Vector2(0.5f, 0f);
			panelRect.sizeDelta = new Vector2(0f, PanelHeight);
			panelRect.anchoredPosition = Vector2.zero;

			var background = panelObject.AddComponent<Image>();
			background.color = new Color(0.02f, 0.03f, 0.08f, 0.55f);
			background.raycastTarget = true;

			var layout = panelObject.AddComponent<HorizontalLayoutGroup>();
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.spacing = 10f;
			layout.padding = new RectOffset(20, 20, 14, 14);
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = true;
			layout.childControlWidth = true;
			layout.childControlHeight = true;

			_timeText = CosmicVisualUtility.CreateUiText(panelRect, "TimeText", TimeFontSize, new Color(0.92f, 0.94f, 1f), TextAnchor.MiddleLeft);
			AddLayoutElement(_timeText.gameObject, TimeLabelMinWidth);

			AddJumpButton(panelRect, "−1月", () => clock.AddMonths(-1));
			AddJumpButton(panelRect, "−1日", () => clock.AddOffset(TimeSpan.FromDays(-1)));
			AddJumpButton(panelRect, "−1時間", () => clock.AddOffset(TimeSpan.FromHours(-1)));
			AddJumpButton(panelRect, "現在へ", () => clock.ResetToNow());
			AddJumpButton(panelRect, "+1時間", () => clock.AddOffset(TimeSpan.FromHours(1)));
			AddJumpButton(panelRect, "+1日", () => clock.AddOffset(TimeSpan.FromDays(1)));
			AddJumpButton(panelRect, "+1月", () => clock.AddMonths(1));

			Button speedButton = CosmicVisualUtility.CreateUiButton(panelRect, "SpeedButton", FormatSpeedLabel(SpeedSteps[0]), ButtonFontSize, CycleSpeed);
			AddLayoutElement(speedButton.gameObject, ButtonMinWidth + 40f);
			_speedButtonLabel = speedButton.GetComponentInChildren<Text>();

			Button pauseButton = CosmicVisualUtility.CreateUiButton(panelRect, "PauseButton", "停止", ButtonFontSize, TogglePause);
			AddLayoutElement(pauseButton.gameObject, ButtonMinWidth);
			_pauseButtonLabel = pauseButton.GetComponentInChildren<Text>();

			_speedIndex = 0;
			clock.TimeScale = SpeedSteps[_speedIndex];
		}

		private void AddJumpButton(RectTransform parent, string label, UnityEngine.Events.UnityAction onClick)
		{
			Button button = CosmicVisualUtility.CreateUiButton(parent, $"Button_{label}", label, ButtonFontSize, onClick);
			AddLayoutElement(button.gameObject, ButtonMinWidth);
		}

		private static void AddLayoutElement(GameObject target, float minWidth)
		{
			var element = target.AddComponent<LayoutElement>();
			element.minWidth = minWidth;
			element.flexibleWidth = 0f;
		}

		private void CycleSpeed()
		{
			_speedIndex = (_speedIndex + 1) % SpeedSteps.Length;
			clock.TimeScale = SpeedSteps[_speedIndex];
			if (_speedButtonLabel != null)
			{
				_speedButtonLabel.text = FormatSpeedLabel(SpeedSteps[_speedIndex]);
			}
		}

		private void TogglePause()
		{
			clock.Paused = !clock.Paused;
			if (_pauseButtonLabel != null)
			{
				_pauseButtonLabel.text = clock.Paused ? "再開" : "停止";
			}
		}

		private static string FormatSpeedLabel(float speed)
		{
			if (speed >= 604800f)
			{
				return "速度 ×7日/秒";
			}

			if (speed >= 86400f)
			{
				return "速度 ×1日/秒";
			}

			if (speed >= 3600f)
			{
				return "速度 ×1時間/秒";
			}

			if (speed >= 60f)
			{
				return "速度 ×60";
			}

			return "速度 ×1";
		}
	}
}

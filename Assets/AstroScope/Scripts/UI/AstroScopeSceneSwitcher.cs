using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AstroScope
{
	/// <summary>
	/// Runtime overlay that lets the player switch between the western and
	/// Eastern cosmic scenes. The switcher bootstraps itself after the first
	/// scene load (no scene editing or prefab placement required), persists
	/// across scene loads, and shows the destination scene on its button.
	/// Works on WebGL builds such as unityroom.
	/// </summary>
	public sealed class AstroScopeSceneSwitcher : MonoBehaviour
	{
		private const string WesternSceneName = "AstroScope_WesternCosmic";
		private const string EasternSceneName = "AstroScope_EasternCosmic";
		private const string WesternCaption = "To Western Horoscope";
		private const string EasternCaption = "To Eastern Astrology";
		private const int CanvasSortingOrder = 100;
		private const int ButtonFontSize = 18;
		private const float ButtonWidth = 230f;
		private const float ButtonHeight = 40f;
		private const float ButtonMargin = 16f;
		private const float ReferenceWidth = 1280f;
		private const float ReferenceHeight = 720f;
		private const KeyCode ToggleKey = KeyCode.Tab;

		private static AstroScopeSceneSwitcher _instance;

		private Text _switchCaption;

		/// <summary>
		/// Creates the persistent switcher instance after the first scene load.
		/// Skips creation when either cosmic scene is missing from the build
		/// profile scene list, so incomplete builds fail gracefully.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Bootstrap()
		{
			if (_instance != null)
			{
				return;
			}

			if (!Application.CanStreamedLevelBeLoaded(WesternSceneName) ||
				!Application.CanStreamedLevelBeLoaded(EasternSceneName))
			{
				Debug.LogWarning(
					"[AstroScope] Scene switcher disabled: add both cosmic scenes " +
					"to the build profile scene list to enable in-game switching.");
				return;
			}

			var switcherObject = new GameObject(nameof(AstroScopeSceneSwitcher));
			DontDestroyOnLoad(switcherObject);
			_instance = switcherObject.AddComponent<AstroScopeSceneSwitcher>();
		}

		/// <summary>
		/// Loads the cosmic scene that is not currently active.
		/// </summary>
		public void SwitchScene()
		{
			SceneManager.LoadScene(GetDestinationSceneName());
		}

		private void Awake()
		{
			BuildOverlay();
			RefreshCaption();
			EnsureEventSystem();
			SceneManager.sceneLoaded += HandleSceneLoaded;
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= HandleSceneLoaded;
			if (_instance == this)
			{
				_instance = null;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(ToggleKey))
			{
				SwitchScene();
			}
		}

		private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			RefreshCaption();
			EnsureEventSystem();
		}

		private static string GetDestinationSceneName()
		{
			return SceneManager.GetActiveScene().name == WesternSceneName
				? EasternSceneName
				: WesternSceneName;
		}

		private void BuildOverlay()
		{
			var canvasObject = new GameObject("SceneSwitcherCanvas", typeof(RectTransform));
			canvasObject.transform.SetParent(transform, false);

			var canvas = canvasObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = CanvasSortingOrder;

			var scaler = canvasObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
			scaler.matchWidthOrHeight = 0.5f;
			canvasObject.AddComponent<GraphicRaycaster>();

			Button switchButton = CosmicVisualUtility.CreateUiButton(
				(RectTransform)canvasObject.transform,
				"SwitchSceneButton",
				string.Empty,
				ButtonFontSize,
				SwitchScene);

			var buttonRect = (RectTransform)switchButton.transform;
			buttonRect.anchorMin = Vector2.one;
			buttonRect.anchorMax = Vector2.one;
			buttonRect.pivot = Vector2.one;
			buttonRect.anchoredPosition = new Vector2(-ButtonMargin, -ButtonMargin);
			buttonRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

			_switchCaption = switchButton.GetComponentInChildren<Text>();
		}

		private void RefreshCaption()
		{
			if (_switchCaption == null)
			{
				return;
			}

			_switchCaption.text = GetDestinationSceneName() == WesternSceneName
				? WesternCaption
				: EasternCaption;
		}

		private static void EnsureEventSystem()
		{
			if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null)
			{
				return;
			}

			var eventSystemObject = new GameObject("EventSystem");
			eventSystemObject.AddComponent<EventSystem>();
			eventSystemObject.AddComponent<StandaloneInputModule>();
		}
	}
}

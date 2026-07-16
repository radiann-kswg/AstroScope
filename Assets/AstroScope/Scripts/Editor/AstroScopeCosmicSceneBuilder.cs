using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AstroScope.EditorTools
{
	/// <summary>
	/// Editor utility that generates the cosmic visualization scenes
	/// (western horoscope and Eastern astrology) with camera, lighting,
	/// URP post-processing, UI, and fully wired view components.
	/// </summary>
	public static class AstroScopeCosmicSceneBuilder
	{
		private const string SceneFolderParent = "Assets/AstroScope/Scene";
		private const string SceneFolder = "Assets/AstroScope/Scene/Cosmic";
		private const string VolumeProfilePath = SceneFolder + "/CosmicVolumeProfile.asset";
		private const string WesternSceneName = "AstroScope_WesternCosmic";
		private const string EasternSceneName = "AstroScope_EasternCosmic";

		/// <summary>
		/// Builds the western horoscope cosmic scene and saves it under the Cosmic scene folder.
		/// </summary>
		[MenuItem("AstroScope/Build Western Cosmic Scene")]
		public static void BuildWesternScene()
		{
			BuildScene(true);
		}

		/// <summary>
		/// Builds the Eastern astrology cosmic scene and saves it under the Cosmic scene folder.
		/// </summary>
		[MenuItem("AstroScope/Build Eastern Cosmic Scene")]
		public static void BuildEasternScene()
		{
			BuildScene(false);
		}

		/// <summary>
		/// Builds both cosmic scenes in sequence. The Eastern scene remains open afterwards.
		/// </summary>
		[MenuItem("AstroScope/Build Both Cosmic Scenes")]
		public static void BuildBothScenes()
		{
			BuildScene(true);
			BuildScene(false);
		}

		private static void BuildScene(bool western)
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return;
			}

			EnsureFolders();
			VolumeProfile profile = EnsureVolumeProfile();

			Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

			ConfigureRenderSettings(western);
			GameObject clockObject = CreateClock();
			CreateCamera();
			CreateAmbientLight(western);
			CreateVolume(profile);
			CreateStarfield();
			Canvas canvas = CreateUiCanvas();
			CreateEventSystem();

			var clock = clockObject.GetComponent<AstroClockController>();

			var timePanel = canvas.gameObject.AddComponent<AstroTimeControlPanel>();
			timePanel.clock = clock;
			timePanel.canvas = canvas;

			if (western)
			{
				var viewObject = new GameObject("WesternHoroscopeView");
				var view = viewObject.AddComponent<WesternHoroscopeView>();
				view.clock = clock;
				view.infoCanvas = canvas;
			}
			else
			{
				var viewObject = new GameObject("EasternAstrologyView");
				var view = viewObject.AddComponent<EasternAstrologyView>();
				view.clock = clock;
				view.infoCanvas = canvas;
			}

			string sceneName = western ? WesternSceneName : EasternSceneName;
			string scenePath = $"{SceneFolder}/{sceneName}.unity";
			bool saved = EditorSceneManager.SaveScene(scene, scenePath);
			if (saved)
			{
				Debug.Log($"[AstroScope] Cosmic scene generated: {scenePath}");
			}
			else
			{
				Debug.LogError($"[AstroScope] Failed to save cosmic scene: {scenePath}");
			}
		}

		private static void EnsureFolders()
		{
			if (!AssetDatabase.IsValidFolder(SceneFolderParent))
			{
				AssetDatabase.CreateFolder("Assets/AstroScope", "Scene");
			}

			if (!AssetDatabase.IsValidFolder(SceneFolder))
			{
				AssetDatabase.CreateFolder(SceneFolderParent, "Cosmic");
			}
		}

		private static VolumeProfile EnsureVolumeProfile()
		{
			var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
			if (existing != null)
			{
				return existing;
			}

			var profile = ScriptableObject.CreateInstance<VolumeProfile>();
			AssetDatabase.CreateAsset(profile, VolumeProfilePath);

			var bloom = profile.Add<Bloom>();
			bloom.name = "Bloom";
			bloom.intensity.Override(1.35f);
			bloom.threshold.Override(0.85f);
			bloom.scatter.Override(0.65f);
			AssetDatabase.AddObjectToAsset(bloom, profile);

			var vignette = profile.Add<Vignette>();
			vignette.name = "Vignette";
			vignette.intensity.Override(0.32f);
			vignette.smoothness.Override(0.4f);
			AssetDatabase.AddObjectToAsset(vignette, profile);

			AssetDatabase.SaveAssets();
			return profile;
		}

		private static void ConfigureRenderSettings(bool western)
		{
			RenderSettings.skybox = null;
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = western
				? new Color(0.10f, 0.10f, 0.18f)
				: new Color(0.08f, 0.09f, 0.16f);
			RenderSettings.fog = false;
		}

		private static GameObject CreateClock()
		{
			var clockObject = new GameObject("AstroClock");
			clockObject.AddComponent<AstroClockController>();
			return clockObject;
		}

		private static void CreateCamera()
		{
			var cameraObject = new GameObject("Main Camera");
			cameraObject.tag = "MainCamera";

			var camera = cameraObject.AddComponent<Camera>();
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
			camera.fieldOfView = 45f;
			camera.nearClipPlane = 0.1f;
			camera.farClipPlane = 200f;
			camera.allowHDR = true;

			cameraObject.transform.position = new Vector3(0f, 16f, -14f);
			cameraObject.transform.LookAt(new Vector3(0f, 1f, 0f));

			cameraObject.AddComponent<AudioListener>();

			UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
			if (cameraData != null)
			{
				cameraData.renderPostProcessing = true;
				cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
			}
		}

		private static void CreateAmbientLight(bool western)
		{
			var lightObject = new GameObject("Ambient Directional Light");
			var light = lightObject.AddComponent<Light>();
			light.type = LightType.Directional;
			light.color = new Color(0.75f, 0.8f, 1f);

			// The Eastern scene has its own moon-phase directional light,
			// so the ambient key light stays dimmer there.
			light.intensity = western ? 0.55f : 0.25f;
			lightObject.transform.rotation = Quaternion.Euler(55f, -25f, 0f);
		}

		private static void CreateVolume(VolumeProfile profile)
		{
			var volumeObject = new GameObject("Global Volume");
			var volume = volumeObject.AddComponent<Volume>();
			volume.isGlobal = true;
			volume.sharedProfile = profile;
		}

		private static void CreateStarfield()
		{
			var starfieldObject = new GameObject("Starfield");
			starfieldObject.AddComponent<StarfieldRenderer>();
		}

		private static Canvas CreateUiCanvas()
		{
			var canvasObject = new GameObject("UI Canvas");
			var canvas = canvasObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			var scaler = canvasObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.matchWidthOrHeight = 0.5f;

			canvasObject.AddComponent<GraphicRaycaster>();
			return canvas;
		}

		private static void CreateEventSystem()
		{
			var eventSystemObject = new GameObject("EventSystem");
			eventSystemObject.AddComponent<EventSystem>();
			eventSystemObject.AddComponent<StandaloneInputModule>();
		}
	}
}

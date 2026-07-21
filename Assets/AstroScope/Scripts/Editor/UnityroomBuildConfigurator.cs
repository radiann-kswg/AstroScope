using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AstroScope.EditorTools
{
	/// <summary>
	/// Editor utility that applies the WebGL player settings required by unityroom
	/// (Gzip compression, disabled decompression fallback, 16:9 canvas size) and
	/// builds the project into the unityroom upload folder.
	/// </summary>
	public static class UnityroomBuildConfigurator
	{
		private const int WebCanvasWidth = 1280;
		private const int WebCanvasHeight = 720;
		private const string BuildOutputFolder = "Builds/Unityroom";
		private const float BytesPerMegabyte = 1024f * 1024f;

		/// <summary>
		/// Applies the unityroom-required WebGL publishing settings to the project.
		/// unityroom only accepts Gzip-compressed builds, and its official help
		/// recommends keeping the decompression fallback disabled because the
		/// server sends the proper Content-Encoding headers.
		/// </summary>
		[MenuItem("AstroScope/Unityroom/Apply Player Settings")]
		public static void ApplyPlayerSettings()
		{
			PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
			PlayerSettings.WebGL.decompressionFallback = false;
			PlayerSettings.WebGL.dataCaching = true;
			PlayerSettings.defaultWebScreenWidth = WebCanvasWidth;
			PlayerSettings.defaultWebScreenHeight = WebCanvasHeight;

			AssetDatabase.SaveAssets();

			Debug.Log(
				"[AstroScope] Applied unityroom player settings: " +
				$"Compression=Gzip, DecompressionFallback=Off, Canvas={WebCanvasWidth}x{WebCanvasHeight}.");
		}

		/// <summary>
		/// Applies the unityroom player settings and builds the project for the
		/// Web (WebGL) platform into <c>Builds/Unityroom</c>. Upload the whole
		/// output folder (or its four build files) to unityroom.
		/// </summary>
		[MenuItem("AstroScope/Unityroom/Build For Unityroom")]
		public static void BuildForUnityroom()
		{
			if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
			{
				Debug.LogError(
					"[AstroScope] The Web (WebGL) build module is not installed. " +
					"Install it from Unity Hub (Add modules > Web Build Support), then restart the editor.");
				return;
			}

			string[] scenePaths = EditorBuildSettings.scenes
				.Where(scene => scene.enabled)
				.Select(scene => scene.path)
				.ToArray();

			if (scenePaths.Length == 0)
			{
				Debug.LogError(
					"[AstroScope] No enabled scenes found in Build Profiles. " +
					"Add at least one scene before building for unityroom.");
				return;
			}

			ApplyPlayerSettings();
			CosmicWebGlSupportAssets.EnsureSupportAssets();

			BuildPlayerOptions buildOptions = new BuildPlayerOptions
			{
				scenes = scenePaths,
				locationPathName = BuildOutputFolder,
				target = BuildTarget.WebGL,
				options = BuildOptions.None
			};

			try
			{
				BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
				BuildSummary summary = report.summary;

				if (summary.result == BuildResult.Succeeded)
				{
					Debug.Log(
						"[AstroScope] unityroom build succeeded: " +
						$"{Path.GetFullPath(BuildOutputFolder)} " +
						$"({summary.totalSize / BytesPerMegabyte:F1} MB, {summary.totalTime.TotalSeconds:F0} s). " +
						"Upload the whole folder to unityroom.");
				}
				else
				{
					Debug.LogError(
						$"[AstroScope] unityroom build finished with result '{summary.result}' " +
						$"({summary.totalErrors} error(s)). Check the console for details.");
				}
			}
			catch (Exception exception)
			{
				Debug.LogError($"[AstroScope] unityroom build failed with an exception: {exception.Message}");
			}
		}
	}
}

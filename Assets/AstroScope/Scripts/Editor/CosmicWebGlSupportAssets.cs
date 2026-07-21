using UnityEditor;
using UnityEngine;

namespace AstroScope.EditorTools
{
	/// <summary>
	/// Creates the Resources assets that keep the cosmic visualization working
	/// in WebGL builds: template materials that force Unity to include the
	/// shaders (and the variants) used by runtime-generated materials, and the
	/// folder for the embedded Japanese label font.
	/// </summary>
	public static class CosmicWebGlSupportAssets
	{
		private const string AstroScopeFolder = "Assets/AstroScope";
		private const string ResourcesFolder = AstroScopeFolder + "/Resources";
		private const string TemplatesFolder = ResourcesFolder + "/CosmicTemplates";
		private const string FontsFolder = ResourcesFolder + "/Fonts";
		private const string LitTemplatePath = TemplatesFolder + "/CosmicLitTemplate.mat";
		private const string UnlitTemplatePath = TemplatesFolder + "/CosmicUnlitTemplate.mat";
		private const string AdditiveTemplatePath = TemplatesFolder + "/CosmicAdditiveTemplate.mat";
		private const string FontAssetPath = FontsFolder + "/AstroScopeLabelFont.ttf";
		private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
		private const string UrpUnlitShaderName = "Universal Render Pipeline/Unlit";
		private const string AdditiveShaderName = "AstroScope/Additive";
		private const string EmissionKeyword = "_EMISSION";

		/// <summary>
		/// Creates the template materials and Resources folders when missing.
		/// Safe to run repeatedly; existing assets are left untouched.
		/// </summary>
		[MenuItem("AstroScope/Unityroom/Create WebGL Support Assets")]
		public static void EnsureSupportAssets()
		{
			EnsureFolder(AstroScopeFolder, "Resources");
			EnsureFolder(ResourcesFolder, "CosmicTemplates");
			EnsureFolder(ResourcesFolder, "Fonts");

			EnsureLitTemplate();
			EnsureTemplateMaterial(UnlitTemplatePath, UrpUnlitShaderName);
			EnsureTemplateMaterial(AdditiveTemplatePath, AdditiveShaderName);

			AssetDatabase.SaveAssets();

			if (AssetDatabase.LoadAssetAtPath<Font>(FontAssetPath) == null)
			{
				Debug.LogWarning(
					"[AstroScope] Japanese label font not found. Place a TTF licensed for embedding " +
					$"(e.g. Noto Sans JP, SIL OFL) at '{FontAssetPath}' so WebGL builds can draw Japanese text.");
			}
			else
			{
				Debug.Log($"[AstroScope] Embedded label font found: {FontAssetPath}");
			}

			Debug.Log("[AstroScope] WebGL support assets are ready.");
		}

		private static void EnsureFolder(string parentPath, string folderName)
		{
			string folderPath = $"{parentPath}/{folderName}";
			if (!AssetDatabase.IsValidFolder(folderPath))
			{
				AssetDatabase.CreateFolder(parentPath, folderName);
			}
		}

		/// <summary>
		/// Creates the URP Lit template with the emission keyword enabled so the
		/// emissive shader variant used by <c>CreateEmissiveMaterial</c> survives
		/// build-time variant stripping. The default black emission keeps plain
		/// lit copies visually unchanged.
		/// </summary>
		private static void EnsureLitTemplate()
		{
			if (AssetDatabase.LoadAssetAtPath<Material>(LitTemplatePath) != null)
			{
				return;
			}

			Shader shader = Shader.Find(UrpLitShaderName);
			if (shader == null)
			{
				Debug.LogError($"[AstroScope] Shader '{UrpLitShaderName}' was not found; '{LitTemplatePath}' was not created.");
				return;
			}

			var material = new Material(shader);
			material.EnableKeyword(EmissionKeyword);
			material.SetColor("_EmissionColor", Color.black);
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
			AssetDatabase.CreateAsset(material, LitTemplatePath);
		}

		private static void EnsureTemplateMaterial(string assetPath, string shaderName)
		{
			if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null)
			{
				return;
			}

			Shader shader = Shader.Find(shaderName);
			if (shader == null)
			{
				Debug.LogError($"[AstroScope] Shader '{shaderName}' was not found; '{assetPath}' was not created.");
				return;
			}

			AssetDatabase.CreateAsset(new Material(shader), assetPath);
		}
	}
}

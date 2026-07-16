using UnityEngine;
using UnityEngine.UI;

namespace AstroScope
{
	/// <summary>
	/// Shared helpers for building the cosmic visualization scenes:
	/// materials, fonts, world-space labels, procedural meshes, and UI primitives.
	/// </summary>
	public static class CosmicVisualUtility
	{
		private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
		private const string UrpUnlitShaderName = "Universal Render Pipeline/Unlit";
		private const string AdditiveShaderName = "AstroScope/Additive";
		private const string FallbackLitShaderName = "Standard";
		private const string FallbackUnlitShaderName = "Unlit/Color";
		private const int SoftCircleTextureSize = 64;
		private const int LabelFontRequestSize = 48;

		private static readonly string[] JapaneseOsFontNames =
		{
			"Yu Gothic UI",
			"Meiryo UI",
			"Meiryo",
			"Hiragino Kaku Gothic ProN",
			"Hiragino Sans"
		};

		private static Font _labelFont;
		private static Texture2D _softCircleTexture;

		/// <summary>
		/// Returns a dynamic font capable of rendering Japanese labels, falling back to the built-in font.
		/// </summary>
		/// <returns>Font usable for both TextMesh and UGUI Text.</returns>
		public static Font GetLabelFont()
		{
			if (_labelFont != null)
			{
				return _labelFont;
			}

			foreach (string fontName in JapaneseOsFontNames)
			{
				try
				{
					Font candidate = Font.CreateDynamicFontFromOSFont(fontName, LabelFontRequestSize);
					if (candidate != null)
					{
						_labelFont = candidate;
						return _labelFont;
					}
				}
				catch (System.ArgumentException)
				{
					// The OS font was unavailable; try the next candidate.
				}
			}

			_labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			return _labelFont;
		}

		/// <summary>
		/// Returns a shared soft radial-gradient texture used by stars and glow billboards.
		/// </summary>
		/// <returns>Square texture with alpha fading from center to edge.</returns>
		public static Texture2D GetSoftCircleTexture()
		{
			if (_softCircleTexture != null)
			{
				return _softCircleTexture;
			}

			_softCircleTexture = new Texture2D(SoftCircleTextureSize, SoftCircleTextureSize, TextureFormat.RGBA32, false)
			{
				wrapMode = TextureWrapMode.Clamp,
				name = "AstroSoftCircle"
			};

			float half = (SoftCircleTextureSize - 1) * 0.5f;
			for (int y = 0; y < SoftCircleTextureSize; y++)
			{
				for (int x = 0; x < SoftCircleTextureSize; x++)
				{
					float distance = Vector2.Distance(new Vector2(x, y), new Vector2(half, half)) / half;
					float alpha = Mathf.Clamp01(1f - distance);
					alpha *= alpha;
					_softCircleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
				}
			}

			_softCircleTexture.Apply();
			return _softCircleTexture;
		}

		/// <summary>
		/// Creates a lit material with an emissive glow, preferring the URP Lit shader.
		/// </summary>
		/// <param name="baseColor">Albedo color of the surface.</param>
		/// <param name="emissionColor">Emission color (HDR values allowed for bloom).</param>
		/// <returns>Configured material instance.</returns>
		public static Material CreateEmissiveMaterial(Color baseColor, Color emissionColor)
		{
			Material material = CreateLitMaterial(baseColor);
			material.EnableKeyword("_EMISSION");
			material.SetColor("_EmissionColor", emissionColor);
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
			return material;
		}

		/// <summary>
		/// Creates a plain lit material, preferring the URP Lit shader.
		/// </summary>
		/// <param name="baseColor">Albedo color of the surface.</param>
		/// <returns>Configured material instance.</returns>
		public static Material CreateLitMaterial(Color baseColor)
		{
			Shader shader = Shader.Find(UrpLitShaderName);
			if (shader == null)
			{
				shader = Shader.Find(FallbackLitShaderName);
			}

			var material = new Material(shader);
			SetBaseColor(material, baseColor);
			return material;
		}

		/// <summary>
		/// Creates an unlit material, preferring the URP Unlit shader.
		/// </summary>
		/// <param name="color">Flat color of the material.</param>
		/// <returns>Configured material instance.</returns>
		public static Material CreateUnlitMaterial(Color color)
		{
			Shader shader = Shader.Find(UrpUnlitShaderName);
			if (shader == null)
			{
				shader = Shader.Find(FallbackUnlitShaderName);
			}

			var material = new Material(shader);
			SetBaseColor(material, color);
			return material;
		}

		/// <summary>
		/// Creates an additive glow material using the bundled AstroScope/Additive shader.
		/// </summary>
		/// <param name="tint">Tint color; alpha scales the glow strength.</param>
		/// <param name="useSoftTexture">When true, applies the soft radial texture (for billboards and stars).</param>
		/// <returns>Configured material instance.</returns>
		public static Material CreateAdditiveMaterial(Color tint, bool useSoftTexture)
		{
			Shader shader = Shader.Find(AdditiveShaderName);
			if (shader == null)
			{
				// Fallback keeps the scene functional even if the custom shader has not imported yet.
				return CreateUnlitMaterial(tint);
			}

			var material = new Material(shader);
			material.SetColor("_TintColor", tint);
			if (useSoftTexture)
			{
				material.SetTexture("_MainTex", GetSoftCircleTexture());
			}

			return material;
		}

		/// <summary>
		/// Creates a camera-facing world-space text label.
		/// </summary>
		/// <param name="parent">Parent transform of the label.</param>
		/// <param name="objectName">Name of the created GameObject.</param>
		/// <param name="text">Initial text.</param>
		/// <param name="localPosition">Local position relative to the parent.</param>
		/// <param name="fontSize">Font size in font units.</param>
		/// <param name="characterSize">TextMesh character size (world scale).</param>
		/// <param name="color">Text color.</param>
		/// <returns>The created TextMesh component.</returns>
		public static TextMesh CreateWorldLabel(Transform parent, string objectName, string text, Vector3 localPosition, int fontSize, float characterSize, Color color)
		{
			var labelObject = new GameObject(objectName);
			labelObject.transform.SetParent(parent, false);
			labelObject.transform.localPosition = localPosition;

			Font font = GetLabelFont();
			var textMesh = labelObject.AddComponent<TextMesh>();
			textMesh.font = font;
			textMesh.fontSize = fontSize;
			textMesh.characterSize = characterSize;
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;
			textMesh.color = color;
			textMesh.text = text;

			var meshRenderer = labelObject.GetComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = font.material;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;

			labelObject.AddComponent<Billboard>();
			return textMesh;
		}

		/// <summary>
		/// Creates and configures a line renderer for chart lines.
		/// </summary>
		/// <param name="parent">Parent transform of the line.</param>
		/// <param name="objectName">Name of the created GameObject.</param>
		/// <param name="color">Line color (used additively).</param>
		/// <param name="width">Line width in world units.</param>
		/// <returns>The created LineRenderer component.</returns>
		public static LineRenderer CreateLine(Transform parent, string objectName, Color color, float width)
		{
			var lineObject = new GameObject(objectName);
			lineObject.transform.SetParent(parent, false);

			var line = lineObject.AddComponent<LineRenderer>();
			line.useWorldSpace = false;
			line.positionCount = 2;
			line.startWidth = width;
			line.endWidth = width;
			line.sharedMaterial = CreateAdditiveMaterial(color, false);
			line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			line.receiveShadows = false;
			return line;
		}

		/// <summary>
		/// Builds a flat ring-sector mesh on the XZ plane (counterclockwise seen from +Y).
		/// </summary>
		/// <param name="innerRadius">Inner radius of the sector.</param>
		/// <param name="outerRadius">Outer radius of the sector.</param>
		/// <param name="startDegrees">Start angle in degrees.</param>
		/// <param name="endDegrees">End angle in degrees (must be greater than start).</param>
		/// <param name="segments">Number of arc subdivisions (minimum 1).</param>
		/// <returns>The generated mesh.</returns>
		public static Mesh BuildRingSectorMesh(float innerRadius, float outerRadius, float startDegrees, float endDegrees, int segments)
		{
			if (segments < 1)
			{
				throw new System.ArgumentOutOfRangeException(nameof(segments), "Segments must be at least 1.");
			}

			if (endDegrees <= startDegrees)
			{
				throw new System.ArgumentException("End angle must be greater than start angle.", nameof(endDegrees));
			}

			int vertexCount = (segments + 1) * 2;
			var vertices = new Vector3[vertexCount];
			var uv = new Vector2[vertexCount];
			var normals = new Vector3[vertexCount];
			var triangles = new int[segments * 6];

			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				float angle = Mathf.Lerp(startDegrees, endDegrees, t) * Mathf.Deg2Rad;
				var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
				vertices[i * 2] = direction * innerRadius;
				vertices[i * 2 + 1] = direction * outerRadius;
				uv[i * 2] = new Vector2(t, 0f);
				uv[i * 2 + 1] = new Vector2(t, 1f);
				normals[i * 2] = Vector3.up;
				normals[i * 2 + 1] = Vector3.up;
			}

			for (int i = 0; i < segments; i++)
			{
				int baseIndex = i * 2;
				int triangleIndex = i * 6;
				triangles[triangleIndex] = baseIndex;
				triangles[triangleIndex + 1] = baseIndex + 3;
				triangles[triangleIndex + 2] = baseIndex + 1;
				triangles[triangleIndex + 3] = baseIndex;
				triangles[triangleIndex + 4] = baseIndex + 2;
				triangles[triangleIndex + 5] = baseIndex + 3;
			}

			var mesh = new Mesh
			{
				name = "RingSector",
				vertices = vertices,
				uv = uv,
				normals = normals,
				triangles = triangles
			};
			mesh.RecalculateBounds();
			return mesh;
		}

		/// <summary>
		/// Converts an ecliptic longitude in degrees to a unit direction on the XZ plane.
		/// </summary>
		/// <param name="longitudeDegrees">Ecliptic longitude in degrees.</param>
		/// <returns>Unit direction (counterclockwise seen from +Y, 0° = +X).</returns>
		public static Vector3 DirectionFromLongitude(double longitudeDegrees)
		{
			float radians = (float)longitudeDegrees * Mathf.Deg2Rad;
			return new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));
		}

		/// <summary>
		/// Creates a UGUI text element under the given parent.
		/// </summary>
		/// <param name="parent">Parent RectTransform.</param>
		/// <param name="objectName">Name of the created GameObject.</param>
		/// <param name="fontSize">Font size in points.</param>
		/// <param name="color">Text color.</param>
		/// <param name="alignment">Text anchor alignment.</param>
		/// <returns>The created Text component.</returns>
		public static Text CreateUiText(RectTransform parent, string objectName, int fontSize, Color color, TextAnchor alignment)
		{
			var textObject = new GameObject(objectName, typeof(RectTransform));
			var rect = (RectTransform)textObject.transform;
			rect.SetParent(parent, false);

			var text = textObject.AddComponent<Text>();
			text.font = GetLabelFont();
			text.fontSize = fontSize;
			text.color = color;
			text.alignment = alignment;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.raycastTarget = false;
			return text;
		}

		/// <summary>
		/// Creates a UGUI button with a text label under the given parent.
		/// </summary>
		/// <param name="parent">Parent RectTransform.</param>
		/// <param name="objectName">Name of the created GameObject.</param>
		/// <param name="label">Button caption.</param>
		/// <param name="fontSize">Caption font size.</param>
		/// <param name="onClick">Click handler.</param>
		/// <returns>The created Button component.</returns>
		public static Button CreateUiButton(RectTransform parent, string objectName, string label, int fontSize, UnityEngine.Events.UnityAction onClick)
		{
			var buttonObject = new GameObject(objectName, typeof(RectTransform));
			var rect = (RectTransform)buttonObject.transform;
			rect.SetParent(parent, false);

			var image = buttonObject.AddComponent<Image>();
			image.color = new Color(0.16f, 0.18f, 0.32f, 0.9f);

			var button = buttonObject.AddComponent<Button>();
			button.targetGraphic = image;
			if (onClick != null)
			{
				button.onClick.AddListener(onClick);
			}

			Text caption = CreateUiText(rect, "Label", fontSize, new Color(0.9f, 0.93f, 1f), TextAnchor.MiddleCenter);
			var captionRect = (RectTransform)caption.transform;
			captionRect.anchorMin = Vector2.zero;
			captionRect.anchorMax = Vector2.one;
			captionRect.offsetMin = Vector2.zero;
			captionRect.offsetMax = Vector2.zero;
			caption.text = label;
			return button;
		}

		private static void SetBaseColor(Material material, Color color)
		{
			if (material.HasProperty("_BaseColor"))
			{
				material.SetColor("_BaseColor", color);
			}

			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", color);
			}
		}
	}
}

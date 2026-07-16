using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AstroScope
{
	/// <summary>
	/// Visualizes the western horoscope as a glowing 3D zodiac wheel:
	/// element-colored sign sectors, planet orbs placed by real ephemeris longitudes,
	/// house cusp lines, ascendant/midheaven markers, and an information panel.
	/// </summary>
	public class WesternHoroscopeView : MonoBehaviour
	{
		private const float WheelInnerRadius = 9.2f;
		private const float WheelOuterRadius = 10.8f;
		private const float SignLabelRadius = 12.1f;
		private const float SignSectorGapDegrees = 0.5f;
		private const int SectorArcSegments = 12;
		private const float PlanetBaseOrbitRadius = 3.1f;
		private const float PlanetOrbitRadiusStep = 0.58f;
		private const float PlanetOrbScale = 0.42f;
		private const float PlanetGlowScale = 1.7f;
		private const float PlanetLabelHeight = 0.85f;
		private const float PlanetMotionSharpness = 4f;
		private const float HouseLineWidth = 0.03f;
		private const float AxisLineWidth = 0.09f;
		private const float SignDegreesSpan = 30f;

		/// <summary>Clock providing the simulated time and observer location. Assigned by the scene builder.</summary>
		public AstroClockController clock;

		/// <summary>Canvas hosting the information panel. Assigned by the scene builder.</summary>
		public Canvas infoCanvas;

		[SerializeField]
		private AstroLanguage language = AstroLanguage.Japanese;

		[SerializeField]
		private float recomputeIntervalSimulatedSeconds = 60f;

		private readonly AstroScopeService _service = new();
		private readonly Dictionary<PlanetId, PlanetVisual> _planetVisuals = new();
		private readonly List<LineRenderer> _houseLines = new();
		private readonly StringBuilder _infoBuilder = new(1024);

		private WesternHoroscopeResult _latestResult;
		private DateTime _lastComputedUtc = DateTime.MinValue;
		private LineRenderer _ascendantLine;
		private LineRenderer _midheavenLine;
		private TextMesh _ascendantLabel;
		private TextMesh _midheavenLabel;
		private Text _infoText;
		private bool _recomputeRequested;

		private sealed class PlanetVisual
		{
			public Transform Root;
			public TextMesh Label;
			public float OrbitRadius;
			public float CurrentLongitude;
			public float TargetLongitude;
			public float TargetHeight;
		}

		private void Start()
		{
			if (clock == null)
			{
				clock = FindFirstObjectByType<AstroClockController>();
			}

			if (clock == null)
			{
				Debug.LogError("[AstroScope] WesternHoroscopeView requires an AstroClockController in the scene.");
				enabled = false;
				return;
			}

			BuildZodiacWheel();
			BuildHouseLines();
			BuildInfoPanel();
			Recompute(true);
		}

		private void OnEnable()
		{
			if (clock != null)
			{
				clock.TimeJumped += RequestRecompute;
			}
		}

		private void OnDisable()
		{
			if (clock != null)
			{
				clock.TimeJumped -= RequestRecompute;
			}
		}

		private void Update()
		{
			if (clock == null)
			{
				return;
			}

			double elapsedSimulatedSeconds = Math.Abs((clock.CurrentUtc - _lastComputedUtc).TotalSeconds);
			if (_recomputeRequested || elapsedSimulatedSeconds >= recomputeIntervalSimulatedSeconds)
			{
				Recompute(_recomputeRequested);
				_recomputeRequested = false;
			}

			AnimatePlanets();
		}

		/// <summary>
		/// Requests an immediate recomputation on the next frame (used after time jumps).
		/// </summary>
		public void RequestRecompute()
		{
			_recomputeRequested = true;
		}

		private void Recompute(bool snapInstantly)
		{
			try
			{
				_latestResult = _service.ComputeWestern(clock.CurrentUtc, clock.LatitudeDegrees, clock.LongitudeDegrees);
			}
			catch (Exception exception)
			{
				Debug.LogError($"[AstroScope] Western horoscope computation failed: {exception.Message}");
				return;
			}

			_lastComputedUtc = clock.CurrentUtc;

			int orbitIndex = 0;
			foreach (CelestialPosition body in _latestResult.Bodies)
			{
				if (body.Body == PlanetId.Earth)
				{
					continue;
				}

				if (!_planetVisuals.TryGetValue(body.Body, out PlanetVisual visual))
				{
					visual = CreatePlanetVisual(body.Body, PlanetBaseOrbitRadius + orbitIndex * PlanetOrbitRadiusStep);
					_planetVisuals.Add(body.Body, visual);
				}

				visual.TargetLongitude = (float)body.EclipticLongitude;
				visual.TargetHeight = Mathf.Clamp((float)body.EclipticLatitude, -8f, 8f) * 0.06f + 0.25f;
				if (snapInstantly)
				{
					visual.CurrentLongitude = visual.TargetLongitude;
				}

				orbitIndex++;
			}

			UpdateHouseLines();
			UpdateInfoPanel();
		}

		private void AnimatePlanets()
		{
			float blend = 1f - Mathf.Exp(-PlanetMotionSharpness * Time.deltaTime);
			foreach (PlanetVisual visual in _planetVisuals.Values)
			{
				visual.CurrentLongitude = Mathf.LerpAngle(visual.CurrentLongitude, visual.TargetLongitude, blend);
				Vector3 direction = CosmicVisualUtility.DirectionFromLongitude(visual.CurrentLongitude);
				visual.Root.localPosition = direction * visual.OrbitRadius + Vector3.up * visual.TargetHeight;
			}
		}

		private void BuildZodiacWheel()
		{
			var wheelRoot = new GameObject("ZodiacWheel");
			wheelRoot.transform.SetParent(transform, false);

			foreach (ZodiacSign sign in Enum.GetValues(typeof(ZodiacSign)))
			{
				float startDegrees = (int)sign * SignDegreesSpan + SignSectorGapDegrees;
				float endDegrees = ((int)sign + 1) * SignDegreesSpan - SignSectorGapDegrees;

				var sectorObject = new GameObject($"Sector_{sign}");
				sectorObject.transform.SetParent(wheelRoot.transform, false);

				var meshFilter = sectorObject.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = CosmicVisualUtility.BuildRingSectorMesh(WheelInnerRadius, WheelOuterRadius, startDegrees, endDegrees, SectorArcSegments);

				var meshRenderer = sectorObject.AddComponent<MeshRenderer>();
				Color elementColor = GetElementColor(sign);
				meshRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(elementColor * 0.45f, false);
				meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				meshRenderer.receiveShadows = false;

				float midDegrees = ((int)sign + 0.5f) * SignDegreesSpan;
				Vector3 labelPosition = CosmicVisualUtility.DirectionFromLongitude(midDegrees) * SignLabelRadius + Vector3.up * 0.4f;
				string signName = AstroLocalization.GetZodiacName(sign, language);
				CosmicVisualUtility.CreateWorldLabel(wheelRoot.transform, $"Label_{sign}", signName, labelPosition, 44, 0.14f, elementColor + new Color(0.3f, 0.3f, 0.3f));

				LineRenderer boundary = CosmicVisualUtility.CreateLine(wheelRoot.transform, $"Boundary_{sign}", new Color(0.55f, 0.6f, 0.95f, 0.5f), 0.025f);
				Vector3 boundaryDirection = CosmicVisualUtility.DirectionFromLongitude((int)sign * SignDegreesSpan);
				boundary.SetPosition(0, boundaryDirection * WheelInnerRadius);
				boundary.SetPosition(1, boundaryDirection * WheelOuterRadius);
			}
		}

		private void BuildHouseLines()
		{
			var houseRoot = new GameObject("Houses");
			houseRoot.transform.SetParent(transform, false);

			const int houseCount = 12;
			for (int i = 0; i < houseCount; i++)
			{
				LineRenderer line = CosmicVisualUtility.CreateLine(houseRoot.transform, $"HouseCusp_{i + 1}", new Color(0.45f, 0.5f, 0.8f, 0.35f), HouseLineWidth);
				line.SetPosition(0, Vector3.zero);
				line.SetPosition(1, Vector3.right * WheelInnerRadius);
				_houseLines.Add(line);
			}

			_ascendantLine = CosmicVisualUtility.CreateLine(houseRoot.transform, "AscendantAxis", new Color(1f, 0.84f, 0.35f, 0.9f), AxisLineWidth);
			_midheavenLine = CosmicVisualUtility.CreateLine(houseRoot.transform, "MidheavenAxis", new Color(0.6f, 0.9f, 1f, 0.9f), AxisLineWidth);

			_ascendantLabel = CosmicVisualUtility.CreateWorldLabel(houseRoot.transform, "AscendantLabel", "ASC", Vector3.right * (WheelOuterRadius + 0.8f), 40, 0.13f, new Color(1f, 0.87f, 0.45f));
			_midheavenLabel = CosmicVisualUtility.CreateWorldLabel(houseRoot.transform, "MidheavenLabel", "MC", Vector3.forward * (WheelOuterRadius + 0.8f), 40, 0.13f, new Color(0.65f, 0.92f, 1f));
		}

		private PlanetVisual CreatePlanetVisual(PlanetId planet, float orbitRadius)
		{
			var root = new GameObject($"Planet_{planet}");
			root.transform.SetParent(transform, false);

			Color planetColor = GetPlanetColor(planet);

			GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			orb.name = "Orb";
			orb.transform.SetParent(root.transform, false);
			orb.transform.localScale = Vector3.one * PlanetOrbScale;
			UnityEngine.Object.Destroy(orb.GetComponent<Collider>());
			var orbRenderer = orb.GetComponent<MeshRenderer>();
			orbRenderer.sharedMaterial = CosmicVisualUtility.CreateEmissiveMaterial(planetColor * 0.6f, planetColor * 2.2f);
			orbRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			var glowObject = new GameObject("Glow");
			glowObject.transform.SetParent(root.transform, false);
			glowObject.transform.localScale = Vector3.one * PlanetGlowScale;
			var glowFilter = glowObject.AddComponent<MeshFilter>();
			glowFilter.sharedMesh = BuildQuadMesh();
			var glowRenderer = glowObject.AddComponent<MeshRenderer>();
			glowRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(new Color(planetColor.r, planetColor.g, planetColor.b, 0.55f), true);
			glowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			glowObject.AddComponent<Billboard>();

			if (planet == PlanetId.Sun)
			{
				var lightObject = new GameObject("SunLight");
				lightObject.transform.SetParent(root.transform, false);
				var sunLight = lightObject.AddComponent<Light>();
				sunLight.type = LightType.Point;
				sunLight.range = 14f;
				sunLight.intensity = 2.4f;
				sunLight.color = new Color(1f, 0.9f, 0.7f);
			}

			string planetName = AstroLocalization.GetPlanetName(planet, language);
			TextMesh label = CosmicVisualUtility.CreateWorldLabel(root.transform, "Label", planetName, Vector3.up * PlanetLabelHeight, 36, 0.11f, Color.white);

			var orbitLine = new GameObject("OrbitRing");
			orbitLine.transform.SetParent(transform, false);
			var orbitRenderer = orbitLine.AddComponent<LineRenderer>();
			const int orbitSegments = 96;
			orbitRenderer.useWorldSpace = false;
			orbitRenderer.loop = true;
			orbitRenderer.positionCount = orbitSegments;
			orbitRenderer.startWidth = 0.015f;
			orbitRenderer.endWidth = 0.015f;
			orbitRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(new Color(planetColor.r, planetColor.g, planetColor.b, 0.10f), false);
			orbitRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			for (int i = 0; i < orbitSegments; i++)
			{
				float angle = i * (360f / orbitSegments);
				orbitRenderer.SetPosition(i, CosmicVisualUtility.DirectionFromLongitude(angle) * orbitRadius);
			}

			return new PlanetVisual
			{
				Root = root.transform,
				Label = label,
				OrbitRadius = orbitRadius,
				CurrentLongitude = 0f,
				TargetLongitude = 0f,
				TargetHeight = 0.25f
			};
		}

		private void UpdateHouseLines()
		{
			if (_latestResult == null)
			{
				return;
			}

			HouseCalculationResult houses = _latestResult.Houses;
			for (int i = 0; i < _houseLines.Count && i < houses.Houses.Count; i++)
			{
				Vector3 direction = CosmicVisualUtility.DirectionFromLongitude(houses.Houses[i].Longitude);
				_houseLines[i].SetPosition(0, direction * 0.6f);
				_houseLines[i].SetPosition(1, direction * WheelInnerRadius);
			}

			Vector3 ascendantDirection = CosmicVisualUtility.DirectionFromLongitude(houses.AscendantDegrees);
			_ascendantLine.SetPosition(0, Vector3.zero);
			_ascendantLine.SetPosition(1, ascendantDirection * WheelOuterRadius);
			_ascendantLabel.transform.localPosition = ascendantDirection * (WheelOuterRadius + 0.8f) + Vector3.up * 0.3f;

			Vector3 midheavenDirection = CosmicVisualUtility.DirectionFromLongitude(houses.MidheavenDegrees);
			_midheavenLine.SetPosition(0, Vector3.zero);
			_midheavenLine.SetPosition(1, midheavenDirection * WheelOuterRadius);
			_midheavenLabel.transform.localPosition = midheavenDirection * (WheelOuterRadius + 0.8f) + Vector3.up * 0.3f;
		}

		private void BuildInfoPanel()
		{
			if (infoCanvas == null)
			{
				infoCanvas = FindFirstObjectByType<Canvas>();
			}

			if (infoCanvas == null)
			{
				return;
			}

			var panelObject = new GameObject("WesternInfoPanel", typeof(RectTransform));
			var panelRect = (RectTransform)panelObject.transform;
			panelRect.SetParent(infoCanvas.transform, false);
			panelRect.anchorMin = new Vector2(0f, 1f);
			panelRect.anchorMax = new Vector2(0f, 1f);
			panelRect.pivot = new Vector2(0f, 1f);
			panelRect.anchoredPosition = new Vector2(24f, -24f);
			panelRect.sizeDelta = new Vector2(560f, 820f);

			var background = panelObject.AddComponent<Image>();
			background.color = new Color(0.02f, 0.03f, 0.08f, 0.45f);
			background.raycastTarget = false;

			_infoText = CosmicVisualUtility.CreateUiText(panelRect, "InfoText", 22, new Color(0.88f, 0.91f, 1f), TextAnchor.UpperLeft);
			var textRect = (RectTransform)_infoText.transform;
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = new Vector2(18f, 18f);
			textRect.offsetMax = new Vector2(-18f, -18f);
		}

		private void UpdateInfoPanel()
		{
			if (_infoText == null || _latestResult == null)
			{
				return;
			}

			_infoBuilder.Clear();
			_infoBuilder.AppendLine("― 西洋ホロスコープ ―");
			_infoBuilder.AppendLine($"{clock.CurrentLocal:yyyy-MM-dd HH:mm} (現地時刻)");
			_infoBuilder.AppendLine();

			string ascendantLabel = AstroLocalization.GetAscendantLabel(language);
			string midheavenLabel = AstroLocalization.GetMidheavenLabel(language);
			_infoBuilder.AppendLine($"{ascendantLabel}: {_latestResult.Houses.AscendantDegrees:F2}°");
			_infoBuilder.AppendLine($"{midheavenLabel}: {_latestResult.Houses.MidheavenDegrees:F2}°");
			_infoBuilder.AppendLine();

			foreach (CelestialPosition body in _latestResult.Bodies)
			{
				if (body.Body == PlanetId.Earth)
				{
					continue;
				}

				string planetName = AstroLocalization.GetPlanetName(body.Body, language);
				string signName = AstroLocalization.GetZodiacName(body.Sign, language);
				_infoBuilder.AppendLine($"{planetName}: {signName} {body.DegreesInSign:F1}°");
			}

			if (_latestResult.Houses.Intercepts.Count > 0)
			{
				_infoBuilder.AppendLine();
				string interceptLabel = AstroLocalization.GetInterceptLabel(language);
				foreach (HouseIntercept intercept in _latestResult.Houses.Intercepts)
				{
					string signName = AstroLocalization.GetZodiacName(intercept.Sign, language);
					string houseLabel = AstroLocalization.GetHouseLabel(intercept.HouseNumber, language);
					_infoBuilder.AppendLine($"{interceptLabel}: {signName} → {houseLabel}");
				}
			}

			_infoText.text = _infoBuilder.ToString();
		}

		private static Mesh BuildQuadMesh()
		{
			var mesh = new Mesh { name = "GlowQuad" };
			mesh.vertices = new[]
			{
				new Vector3(-0.5f, -0.5f, 0f),
				new Vector3(0.5f, -0.5f, 0f),
				new Vector3(0.5f, 0.5f, 0f),
				new Vector3(-0.5f, 0.5f, 0f)
			};
			mesh.uv = new[]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f),
				new Vector2(1f, 1f),
				new Vector2(0f, 1f)
			};
			mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
			mesh.RecalculateBounds();
			return mesh;
		}

		private static Color GetElementColor(ZodiacSign sign)
		{
			switch ((int)sign % 4)
			{
				case 0: // Fire: Aries, Leo, Sagittarius
					return new Color(1f, 0.42f, 0.28f);
				case 1: // Earth: Taurus, Virgo, Capricorn
					return new Color(0.45f, 0.85f, 0.45f);
				case 2: // Air: Gemini, Libra, Aquarius
					return new Color(0.42f, 0.82f, 1f);
				default: // Water: Cancer, Scorpio, Pisces
					return new Color(0.55f, 0.45f, 1f);
			}
		}

		private static Color GetPlanetColor(PlanetId planet)
		{
			switch (planet)
			{
				case PlanetId.Sun:
					return new Color(1f, 0.78f, 0.3f);
				case PlanetId.Moon:
					return new Color(0.85f, 0.88f, 0.95f);
				case PlanetId.Mercury:
					return new Color(0.65f, 0.75f, 0.8f);
				case PlanetId.Venus:
					return new Color(1f, 0.9f, 0.65f);
				case PlanetId.Mars:
					return new Color(1f, 0.4f, 0.3f);
				case PlanetId.Jupiter:
					return new Color(0.95f, 0.7f, 0.45f);
				case PlanetId.Saturn:
					return new Color(0.9f, 0.85f, 0.6f);
				case PlanetId.Uranus:
					return new Color(0.5f, 0.95f, 0.95f);
				case PlanetId.Neptune:
					return new Color(0.35f, 0.55f, 1f);
				case PlanetId.Pluto:
					return new Color(0.75f, 0.55f, 0.95f);
				default:
					return Color.white;
			}
		}
	}
}

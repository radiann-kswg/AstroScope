using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AstroScope
{
	/// <summary>
	/// Visualizes Eastern astrology as a mystical night observatory:
	/// a moon that waxes and wanes with the computed lunar age, a ring of the
	/// twenty-four solar terms with the current term highlighted, a Lo-Shu
	/// nine-star grid, and an information panel for the lunisolar calendar,
	/// sexagenary cycle, and doyou periods.
	/// </summary>
	public class EasternAstrologyView : MonoBehaviour
	{
		private const float TermRingRadius = 9.5f;
		private const float TermLabelRadius = 11.2f;
		private const float TermMarkerScale = 0.22f;
		private const float TermMarkerHighlightScale = 0.55f;
		private const int TermCount = 24;
		private const float MoonHeight = 3.6f;
		private const float MoonScale = 2.6f;
		private const double SynodicMonthDays = 29.530588853;
		private const float NineStarTileSize = 1.7f;
		private const float NineStarTileSpacing = 2.1f;
		private const float MarkerOrbHeight = 1.15f;

		private static readonly int[,] LoShuGrid =
		{
			{ 4, 9, 2 },
			{ 3, 5, 7 },
			{ 8, 1, 6 }
		};

		/// <summary>Clock providing the simulated time and observer location. Assigned by the scene builder.</summary>
		public AstroClockController clock;

		/// <summary>Canvas hosting the information panel. Assigned by the scene builder.</summary>
		public Canvas infoCanvas;

		[SerializeField]
		private AstroLanguage language = AstroLanguage.Japanese;

		private readonly AstroScopeService _service = new();
		private readonly List<Transform> _termMarkers = new();
		private readonly List<Material> _termMarkerMaterials = new();
		private readonly Dictionary<int, Transform> _nineStarTiles = new();
		private readonly StringBuilder _infoBuilder = new(1024);

		private EasternAstrologyResult _latestResult;
		private DateTime _lastComputedLocalDate = DateTime.MinValue;
		private DateTime _lastComputedLocalTime = DateTime.MinValue;
		private Transform _moonRoot;
		private Transform _moonPhaseLightPivot;
		private Transform _yearStarOrb;
		private Transform _monthStarOrb;
		private Transform _dayStarOrb;
		private Text _infoText;
		private int _highlightedTermIndex = -1;
		private bool _recomputeRequested;

		private void Start()
		{
			if (clock == null)
			{
				clock = FindFirstObjectByType<AstroClockController>();
			}

			if (clock == null)
			{
				Debug.LogError("[AstroScope] EasternAstrologyView requires an AstroClockController in the scene.");
				enabled = false;
				return;
			}

			BuildSolarTermRing();
			BuildMoon();
			BuildNineStarGrid();
			BuildInfoPanel();
			Recompute();
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

			DateTime local = clock.CurrentLocal;
			if (_recomputeRequested || local.Date != _lastComputedLocalDate)
			{
				Recompute();
				_recomputeRequested = false;
			}

			AnimateMoonPhase(local);
		}

		/// <summary>
		/// Requests an immediate recomputation on the next frame (used after time jumps).
		/// </summary>
		public void RequestRecompute()
		{
			_recomputeRequested = true;
		}

		private void Recompute()
		{
			DateTime local = clock.CurrentLocal;
			try
			{
				_latestResult = _service.ComputeEastern(local, clock.LongitudeDegrees, clock.TimeZone);
			}
			catch (Exception exception)
			{
				Debug.LogError($"[AstroScope] Eastern astrology computation failed: {exception.Message}");
				return;
			}

			_lastComputedLocalDate = local.Date;
			_lastComputedLocalTime = local;

			UpdateSolarTermHighlight(local);
			UpdateNineStarHighlights();
			UpdateInfoPanel(local);
		}

		private void BuildSolarTermRing()
		{
			var ringRoot = new GameObject("SolarTermRing");
			ringRoot.transform.SetParent(transform, false);

			// Base circle of the ring.
			var circleObject = new GameObject("RingCircle");
			circleObject.transform.SetParent(ringRoot.transform, false);
			var circle = circleObject.AddComponent<LineRenderer>();
			const int circleSegments = 128;
			circle.useWorldSpace = false;
			circle.loop = true;
			circle.positionCount = circleSegments;
			circle.startWidth = 0.03f;
			circle.endWidth = 0.03f;
			circle.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(new Color(0.6f, 0.65f, 1f, 0.35f), false);
			circle.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			for (int i = 0; i < circleSegments; i++)
			{
				float angle = i * (360f / circleSegments);
				circle.SetPosition(i, CosmicVisualUtility.DirectionFromLongitude(angle) * TermRingRadius);
			}

			// One marker + label per solar term, placed by its target solar longitude.
			const float degreesPerTerm = 360f / TermCount;
			const float startOfSpringLongitude = 315f;
			foreach (SolarTermId termId in Enum.GetValues(typeof(SolarTermId)))
			{
				float longitude = startOfSpringLongitude + (int)termId * degreesPerTerm;
				Vector3 direction = CosmicVisualUtility.DirectionFromLongitude(longitude);

				GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				marker.name = $"Term_{termId}";
				marker.transform.SetParent(ringRoot.transform, false);
				marker.transform.localPosition = direction * TermRingRadius + Vector3.up * 0.1f;
				marker.transform.localScale = Vector3.one * TermMarkerScale;
				UnityEngine.Object.Destroy(marker.GetComponent<Collider>());

				var markerRenderer = marker.GetComponent<MeshRenderer>();
				Material markerMaterial = CosmicVisualUtility.CreateEmissiveMaterial(new Color(0.5f, 0.55f, 0.8f), new Color(0.35f, 0.4f, 0.8f));
				markerRenderer.sharedMaterial = markerMaterial;
				markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

				_termMarkers.Add(marker.transform);
				_termMarkerMaterials.Add(markerMaterial);

				string termName = AstroLocalization.GetSolarTermName(termId, language);
				Vector3 labelPosition = direction * TermLabelRadius + Vector3.up * 0.35f;
				CosmicVisualUtility.CreateWorldLabel(ringRoot.transform, $"TermLabel_{termId}", termName, labelPosition, 34, 0.11f, new Color(0.78f, 0.82f, 1f));
			}
		}

		private void BuildMoon()
		{
			var moonRoot = new GameObject("Moon");
			moonRoot.transform.SetParent(transform, false);
			moonRoot.transform.localPosition = Vector3.up * MoonHeight;
			_moonRoot = moonRoot.transform;

			GameObject moonSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			moonSphere.name = "MoonSurface";
			moonSphere.transform.SetParent(moonRoot.transform, false);
			moonSphere.transform.localScale = Vector3.one * MoonScale;
			UnityEngine.Object.Destroy(moonSphere.GetComponent<Collider>());
			var moonRenderer = moonSphere.GetComponent<MeshRenderer>();
			moonRenderer.sharedMaterial = CosmicVisualUtility.CreateLitMaterial(new Color(0.82f, 0.83f, 0.85f));
			moonRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			// Faint halo behind the moon.
			var haloObject = new GameObject("MoonHalo");
			haloObject.transform.SetParent(moonRoot.transform, false);
			haloObject.transform.localScale = Vector3.one * (MoonScale * 2.4f);
			var haloFilter = haloObject.AddComponent<MeshFilter>();
			haloFilter.sharedMesh = BuildQuadMesh();
			var haloRenderer = haloObject.AddComponent<MeshRenderer>();
			haloRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(new Color(0.75f, 0.8f, 1f, 0.18f), true);
			haloRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			haloObject.AddComponent<Billboard>();

			// Directional light dedicated to sculpting the lunar phase.
			var lightPivot = new GameObject("MoonPhaseLightPivot");
			lightPivot.transform.SetParent(moonRoot.transform, false);
			_moonPhaseLightPivot = lightPivot.transform;

			var lightObject = new GameObject("MoonPhaseLight");
			lightObject.transform.SetParent(lightPivot.transform, false);
			var phaseLight = lightObject.AddComponent<Light>();
			phaseLight.type = LightType.Directional;
			phaseLight.intensity = 1.15f;
			phaseLight.color = new Color(1f, 0.97f, 0.9f);
		}

		private void AnimateMoonPhase(DateTime local)
		{
			if (_latestResult == null || _moonPhaseLightPivot == null)
			{
				return;
			}

			double elapsedDays = (local - _lastComputedLocalTime).TotalDays;
			double lunarAge = _latestResult.LunarAgeDays + elapsedDays;
			float phaseFraction = Mathf.Repeat((float)(lunarAge / SynodicMonthDays), 1f);

			// Phase 0 (new moon): light points toward the camera side so the visible face is dark.
			// Phase 0.5 (full moon): light points away from the camera, fully illuminating the visible face.
			float yawDegrees = 180f + phaseFraction * 360f;
			_moonPhaseLightPivot.rotation = Quaternion.Euler(10f, yawDegrees, 0f);
		}

		private void BuildNineStarGrid()
		{
			var gridRoot = new GameObject("NineStarGrid");
			gridRoot.transform.SetParent(transform, false);

			for (int row = 0; row < 3; row++)
			{
				for (int column = 0; column < 3; column++)
				{
					int starNumber = LoShuGrid[row, column];
					var tileObject = new GameObject($"StarTile_{starNumber}");
					tileObject.transform.SetParent(gridRoot.transform, false);
					float x = (column - 1) * NineStarTileSpacing;
					float z = (1 - row) * NineStarTileSpacing;
					tileObject.transform.localPosition = new Vector3(x, 0.02f, z);

					var tileFilter = tileObject.AddComponent<MeshFilter>();
					tileFilter.sharedMesh = BuildTileMesh(NineStarTileSize);

					var tileRenderer = tileObject.AddComponent<MeshRenderer>();
					Color starColor = GetNineStarColor(starNumber);
					tileRenderer.sharedMaterial = CosmicVisualUtility.CreateAdditiveMaterial(starColor * 0.28f, false);
					tileRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

					string starName = AstroLocalization.GetNineStarName(starNumber, language);
					CosmicVisualUtility.CreateWorldLabel(tileObject.transform, "Label", starName, Vector3.up * 0.35f, 30, 0.10f, starColor + new Color(0.35f, 0.35f, 0.35f));

					_nineStarTiles[starNumber] = tileObject.transform;
				}
			}

			_yearStarOrb = CreateMarkerOrb(gridRoot.transform, "YearStarOrb", "年", new Color(1f, 0.85f, 0.4f));
			_monthStarOrb = CreateMarkerOrb(gridRoot.transform, "MonthStarOrb", "月", new Color(0.85f, 0.9f, 1f));
			_dayStarOrb = CreateMarkerOrb(gridRoot.transform, "DayStarOrb", "日", new Color(0.5f, 0.95f, 0.9f));
		}

		private Transform CreateMarkerOrb(Transform parent, string objectName, string caption, Color color)
		{
			var orbRoot = new GameObject(objectName);
			orbRoot.transform.SetParent(parent, false);

			GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			orb.name = "Orb";
			orb.transform.SetParent(orbRoot.transform, false);
			orb.transform.localScale = Vector3.one * 0.3f;
			UnityEngine.Object.Destroy(orb.GetComponent<Collider>());
			var orbRenderer = orb.GetComponent<MeshRenderer>();
			orbRenderer.sharedMaterial = CosmicVisualUtility.CreateEmissiveMaterial(color * 0.6f, color * 2f);
			orbRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			CosmicVisualUtility.CreateWorldLabel(orbRoot.transform, "Label", caption, Vector3.up * 0.45f, 30, 0.11f, color);
			return orbRoot.transform;
		}

		private void UpdateNineStarHighlights()
		{
			if (_latestResult == null)
			{
				return;
			}

			PositionMarkerOrb(_yearStarOrb, _latestResult.Chart.YearStar, 0f);
			PositionMarkerOrb(_monthStarOrb, _latestResult.Chart.MonthStar, 0.35f);
			PositionMarkerOrb(_dayStarOrb, _latestResult.Chart.DayStar, 0.7f);
		}

		private void PositionMarkerOrb(Transform orb, int starNumber, float heightOffset)
		{
			if (orb == null)
			{
				return;
			}

			if (_nineStarTiles.TryGetValue(starNumber, out Transform tile))
			{
				orb.localPosition = tile.localPosition + Vector3.up * (MarkerOrbHeight + heightOffset);
				orb.gameObject.SetActive(true);
			}
			else
			{
				orb.gameObject.SetActive(false);
			}
		}

		private void UpdateSolarTermHighlight(DateTime local)
		{
			if (_latestResult == null || _latestResult.SolarTerms.Count == 0)
			{
				return;
			}

			int currentIndex = -1;
			for (int i = 0; i < _latestResult.SolarTerms.Count; i++)
			{
				if (_latestResult.SolarTerms[i].DateTimeLocal <= local)
				{
					currentIndex = i;
				}
			}

			SolarTermId currentTermId = currentIndex >= 0
				? _latestResult.SolarTerms[currentIndex].Id
				: _latestResult.SolarTerms[0].Id;
			int markerIndex = (int)currentTermId;

			if (_highlightedTermIndex == markerIndex)
			{
				return;
			}

			if (_highlightedTermIndex >= 0 && _highlightedTermIndex < _termMarkers.Count)
			{
				_termMarkers[_highlightedTermIndex].localScale = Vector3.one * TermMarkerScale;
				_termMarkerMaterials[_highlightedTermIndex].SetColor("_EmissionColor", new Color(0.35f, 0.4f, 0.8f));
			}

			if (markerIndex >= 0 && markerIndex < _termMarkers.Count)
			{
				_termMarkers[markerIndex].localScale = Vector3.one * TermMarkerHighlightScale;
				_termMarkerMaterials[markerIndex].SetColor("_EmissionColor", new Color(1f, 0.75f, 0.3f) * 2.5f);
			}

			_highlightedTermIndex = markerIndex;
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

			var panelObject = new GameObject("EasternInfoPanel", typeof(RectTransform));
			var panelRect = (RectTransform)panelObject.transform;
			panelRect.SetParent(infoCanvas.transform, false);
			panelRect.anchorMin = new Vector2(0f, 1f);
			panelRect.anchorMax = new Vector2(0f, 1f);
			panelRect.pivot = new Vector2(0f, 1f);
			panelRect.anchoredPosition = new Vector2(24f, -24f);
			panelRect.sizeDelta = new Vector2(560f, 780f);

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

		private void UpdateInfoPanel(DateTime local)
		{
			if (_infoText == null || _latestResult == null)
			{
				return;
			}

			_infoBuilder.Clear();
			_infoBuilder.AppendLine("― 東洋占星術 ―");
			_infoBuilder.AppendLine($"{local:yyyy-MM-dd HH:mm} (現地時刻)");
			_infoBuilder.AppendLine();

			_infoBuilder.AppendLine($"太陰太陽暦: {AstroLocalization.GetLunisolarString(_latestResult, language)}");
			_infoBuilder.AppendLine($"月齢: {AstroLocalization.GetLunarAgeString(_latestResult.LunarAgeDays, language)}");
			_infoBuilder.AppendLine();

			string yearGanzhi = AstroLocalization.GetSexagenaryName(_latestResult.Sexagenary.Year, language);
			string monthGanzhi = AstroLocalization.GetSexagenaryName(_latestResult.Sexagenary.Month, language);
			string dayGanzhi = AstroLocalization.GetSexagenaryName(_latestResult.Sexagenary.Day, language);
			_infoBuilder.AppendLine($"干支 年: {yearGanzhi}");
			_infoBuilder.AppendLine($"干支 月: {monthGanzhi}");
			_infoBuilder.AppendLine($"干支 日: {dayGanzhi}");
			_infoBuilder.AppendLine();

			_infoBuilder.AppendLine($"九星 年: {AstroLocalization.GetNineStarName(_latestResult.Chart.YearStar, language)}");
			_infoBuilder.AppendLine($"九星 月: {AstroLocalization.GetNineStarName(_latestResult.Chart.MonthStar, language)}");
			_infoBuilder.AppendLine($"九星 日: {AstroLocalization.GetNineStarName(_latestResult.Chart.DayStar, language)}");
			_infoBuilder.AppendLine();

			AppendUpcomingSolarTerms(local);
			AppendDoyouStatus(local);

			_infoText.text = _infoBuilder.ToString();
		}

		private void AppendUpcomingSolarTerms(DateTime local)
		{
			const int upcomingCount = 3;
			int appended = 0;
			_infoBuilder.AppendLine("次の節気:");
			foreach (SolarTermEntry term in _latestResult.SolarTerms)
			{
				if (term.DateTimeLocal < local)
				{
					continue;
				}

				string name = AstroLocalization.GetSolarTermName(term.Id, language);
				_infoBuilder.AppendLine($"  {name}: {term.DateTimeLocal:MM-dd HH:mm}");
				appended++;
				if (appended >= upcomingCount)
				{
					break;
				}
			}

			if (appended == 0)
			{
				_infoBuilder.AppendLine($"  {AstroLocalization.GetNoneLabel(language)}");
			}

			_infoBuilder.AppendLine();
		}

		private void AppendDoyouStatus(DateTime local)
		{
			foreach (DoyouPeriod period in _latestResult.DoyouPeriods)
			{
				if (local >= period.StartLocal && local <= period.EndLocal)
				{
					string name = AstroLocalization.GetDoyouName(period.Season, language);
					_infoBuilder.AppendLine($"土用期間中: {name} ({period.EndLocal:MM-dd} まで)");
					return;
				}
			}

			foreach (DoyouPeriod period in _latestResult.DoyouPeriods)
			{
				if (period.StartLocal > local)
				{
					string name = AstroLocalization.GetDoyouName(period.Season, language);
					_infoBuilder.AppendLine($"次の土用: {name} ({period.StartLocal:MM-dd} から)");
					return;
				}
			}
		}

		private static Mesh BuildTileMesh(float size)
		{
			float half = size * 0.5f;
			var mesh = new Mesh { name = "StarTile" };
			mesh.vertices = new[]
			{
				new Vector3(-half, 0f, -half),
				new Vector3(half, 0f, -half),
				new Vector3(half, 0f, half),
				new Vector3(-half, 0f, half)
			};
			mesh.uv = new[]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f),
				new Vector2(1f, 1f),
				new Vector2(0f, 1f)
			};
			mesh.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
			mesh.triangles = new[] { 0, 3, 2, 0, 2, 1 };
			mesh.RecalculateBounds();
			return mesh;
		}

		private static Mesh BuildQuadMesh()
		{
			var mesh = new Mesh { name = "HaloQuad" };
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

		private static Color GetNineStarColor(int starNumber)
		{
			switch (starNumber)
			{
				case 1: // 一白水星
					return new Color(0.85f, 0.92f, 1f);
				case 2: // 二黒土星
					return new Color(0.55f, 0.45f, 0.35f);
				case 3: // 三碧木星
					return new Color(0.35f, 0.75f, 0.75f);
				case 4: // 四緑木星
					return new Color(0.45f, 0.85f, 0.45f);
				case 5: // 五黄土星
					return new Color(0.95f, 0.8f, 0.3f);
				case 6: // 六白金星
					return new Color(0.95f, 0.95f, 1f);
				case 7: // 七赤金星
					return new Color(1f, 0.45f, 0.4f);
				case 8: // 八白土星
					return new Color(0.9f, 0.85f, 0.7f);
				case 9: // 九紫火星
					return new Color(0.8f, 0.5f, 1f);
				default:
					return Color.white;
			}
		}
	}
}

using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AstroScope
{
	/// <summary>
	/// Sample MonoBehaviour that logs western and Eastern astrology results using the AstroScope services.
	/// </summary>
	public class AstroScpoeMain : MonoBehaviour
	{
		[SerializeField]
		private double latitudeDegrees = 35.6895; // Tokyo

		[SerializeField]
		private double longitudeDegrees = 139.6917;

		[SerializeField]
		private string timeZoneId = "Asia/Tokyo";

		[SerializeField]
		private bool computeOnStart = true;

		[SerializeField]
		private AstroLanguage language = AstroLanguage.Japanese;

		private readonly AstroScopeService _service = new();

		[SerializeField]
		private AstroScopeCelestialPreview CelestialPreview;

		private void Start()
		{
			if (computeOnStart)
			{
				ComputeAndLogHoroscopes();
			}
		}

		[ContextMenu("Compute Horoscopes Now")]
		public void ComputeAndLogHoroscopes()
		{
			TimeZoneInfo timeZone = ResolveTimeZone();
			DateTime nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
			var (western, eastern) = _service.ComputeFull(nowLocal, latitudeDegrees, longitudeDegrees, timeZone);

			Debug.Log($"[AstroScope] Western Horoscope for {nowLocal:u}");
			foreach (var body in western.Bodies)
			{
				string planetName = AstroLocalization.GetPlanetName(body.Body, language);
				string signName = AstroLocalization.GetZodiacName(body.Sign, language);
				Debug.Log($" - {planetName} ({signName} {body.DegreesInSign:F2}°): λ={body.EclipticLongitude:F2}°, β={body.EclipticLatitude:F2}°, Δ={body.DistanceAstronomicalUnits:F4} AU");

				if (CelestialPreview)
				{
					CelestialPreview.CelestialBodiesObject[(int)body.Body].transform.Translate(Quaternion.Euler(0.0f, (float)body.EclipticLongitude, (float)body.EclipticLatitude) * new Vector3((float)body.DistanceAstronomicalUnits, 0.0f, 0.0f) * 42f);
				}
			}

			string ascLabel = AstroLocalization.GetAscendantLabel(language);
			string mcLabel = AstroLocalization.GetMidheavenLabel(language);
			Debug.Log($" {ascLabel}: {western.Houses.AscendantDegrees:F2}°, {mcLabel}: {western.Houses.MidheavenDegrees:F2}°");

			foreach (var house in western.Houses.Houses)
			{
				string houseLabel = AstroLocalization.GetHouseLabel(house.HouseNumber, language);
				string houseSign = western.Houses.HouseCusps.TryGetValue(house.HouseNumber, out var sign)
					? AstroLocalization.GetZodiacName(sign, language)
					: string.Empty;
				Debug.Log($"   {houseLabel}: {house.Longitude:F2}° ({houseSign})");
			}

			string interceptLabel = AstroLocalization.GetInterceptLabel(language);
			if (western.Houses.Intercepts.Count > 0)
			{
				foreach (var intercept in western.Houses.Intercepts)
				{
					string signName = AstroLocalization.GetZodiacName(intercept.Sign, language);
					string houseLabel = AstroLocalization.GetHouseLabel(intercept.HouseNumber, language);
					Debug.Log($"   {interceptLabel}: {signName} → {houseLabel}");
				}
			}
			else
			{
				Debug.Log($"   {interceptLabel}: {AstroLocalization.GetNoneLabel(language)}");
			}

			if (western.MissingCuspSigns.Count > 0)
			{
				string missing = string.Join(", ", western.MissingCuspSigns.Select(sign => AstroLocalization.GetZodiacName(sign, language)));
				Debug.Log($"   {AstroLocalization.GetMissingSignsLabel(language)}: {missing}");
			}

			Debug.Log("[AstroScope] Eastern Astrology");
			Debug.Log($" - Lunisolar(太陽太陰暦): {AstroLocalization.GetLunisolarString(eastern, language)}");
			Debug.Log($" - Lunar Age(月齢): {AstroLocalization.GetLunarAgeString(eastern.LunarAgeDays, language)}");
			Debug.Log($" - Setsubun(節分): {eastern.SetsubunLocal:u}");
			Debug.Log($" - Nine Star Ki(九星気学): Year(年)={AstroLocalization.GetNineStarName(eastern.Chart.YearStar, language)}(), Month(月)={AstroLocalization.GetNineStarName(eastern.Chart.MonthStar, language)}, Day(日)={AstroLocalization.GetNineStarName(eastern.Chart.DayStar, language)}");

			string yearGanzhi = AstroLocalization.GetSexagenaryName(eastern.Sexagenary.Year, language);
			string monthGanzhi = AstroLocalization.GetSexagenaryName(eastern.Sexagenary.Month, language);
			string dayGanzhi = AstroLocalization.GetSexagenaryName(eastern.Sexagenary.Day, language);
			Debug.Log($" - Sexagenary(干支): Year(年)={yearGanzhi}, Month(月)={monthGanzhi}, Day(日)={dayGanzhi}");

			var upcomingTerms = eastern.SolarTerms
				.Where(term => term.DateTimeLocal >= nowLocal)
				.OrderBy(term => term.DateTimeLocal)
				.Take(4)
				.ToList();
			if (upcomingTerms.Count > 0)
			{
				Debug.Log(" - Upcoming Solar Terms(二十四節気):");
				foreach (var term in upcomingTerms)
				{
					string name = AstroLocalization.GetSolarTermName(term.Id, language);
					Debug.Log($"   • {name}: {term.DateTimeLocal:u}");
				}
			}

			Debug.Log(" - Doyou Periods(土用):");
			foreach (var period in eastern.DoyouPeriods)
			{
				string name = AstroLocalization.GetDoyouName(period.Season, language);
				Debug.Log($"   • {name}: {period.StartLocal:u} → {period.EndLocal:u}");
			}
		}

		private TimeZoneInfo ResolveTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			}
			catch (TimeZoneNotFoundException)
			{
				Debug.LogWarning($"[AstroScope] Time zone '{timeZoneId}' not found. Falling back to local system zone.");
				return TimeZoneInfo.Local;
			}
			catch (InvalidTimeZoneException)
			{
				Debug.LogWarning($"[AstroScope] Time zone '{timeZoneId}' invalid. Falling back to local system zone.");
				return TimeZoneInfo.Local;
			}
		}
	}
}

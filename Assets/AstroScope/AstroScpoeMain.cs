using System;
using UnityEngine;

namespace AstroScope
{
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

		private readonly AstroScopeService _service = new();

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
				Debug.Log($" - {body.Body}: λ={body.EclipticLongitude:F2}°, β={body.EclipticLatitude:F2}°, Δ={body.DistanceAstronomicalUnits:F4} AU");
			}
			Debug.Log($"Ascendant: {western.Houses.AscendantDegrees:F2}°, Midheaven: {western.Houses.MidheavenDegrees:F2}°");

			Debug.Log($"[AstroScope] Eastern Astrology");
			Debug.Log($" - Lunisolar(太陽太陰暦): {eastern.LunisolarDate.Year}年 {eastern.LunisolarDate.Month}月 {(eastern.LunisolarDate.IsLeapMonth ? "(閏) " : string.Empty)}{eastern.LunisolarDate.Day}日");
			Debug.Log($" - LunarAge(月齢): {eastern.LunarAgeDays:F2} days");
			Debug.Log($" - Setsubun(節分;local): {eastern.SetsubunLocal:u}");
			Debug.Log($" - Nine Star Ki(九星気学): YearStar={eastern.Chart.YearStar}, MonthStar={eastern.Chart.MonthStar}, DayStar={eastern.Chart.DayStar}");
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

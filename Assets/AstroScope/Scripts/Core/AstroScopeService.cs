using System;

namespace AstroScope
{
	/// <summary>
	/// Facade that exposes western and eastern astrology calculations through a unified API.
	/// </summary>
	public class AstroScopeService
	{
		private readonly WesternHoroscopeCalculator _western = new();
		private readonly EasternAstrologyCalculator _eastern = new();

		/// <summary>Calculates only the western horoscope portion for the supplied UTC instant.</summary>
		public WesternHoroscopeResult ComputeWestern(DateTime dateTimeUtc, double latitudeDegrees, double longitudeDegrees)
		{
			if (dateTimeUtc.Kind != DateTimeKind.Utc)
			{
				dateTimeUtc = DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc);
			}

			return _western.Compute(dateTimeUtc, latitudeDegrees, longitudeDegrees);
		}

		/// <summary>Calculates only the Eastern astrology portion for the supplied local civil time.</summary>
		public EasternAstrologyResult ComputeEastern(DateTime dateTimeLocal, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			return _eastern.Compute(dateTimeLocal, longitudeDegrees, timeZone);
		}

		/// <summary>Calculates both western and Eastern astrology data for a local civil time and coordinates.</summary>
		public (WesternHoroscopeResult Western, EasternAstrologyResult Eastern) ComputeFull(DateTime dateTimeLocal, double latitudeDegrees, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			DateTime dateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateTimeLocal, timeZone);
			var western = _western.Compute(dateTimeUtc, latitudeDegrees, longitudeDegrees);
			var eastern = _eastern.Compute(dateTimeLocal, longitudeDegrees, timeZone);
			return (western, eastern);
		}
	}
}

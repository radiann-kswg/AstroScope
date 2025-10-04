using System;

namespace AstroScope
{
	public class AstroScopeService
	{
		private readonly WesternHoroscopeCalculator _western = new();
		private readonly EasternAstrologyCalculator _eastern = new();

		public WesternHoroscopeResult ComputeWestern(DateTime dateTimeUtc, double latitudeDegrees, double longitudeDegrees)
		{
			if (dateTimeUtc.Kind != DateTimeKind.Utc)
			{
				dateTimeUtc = DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc);
			}

			return _western.Compute(dateTimeUtc, latitudeDegrees, longitudeDegrees);
		}

		public EasternAstrologyResult ComputeEastern(DateTime dateTimeLocal, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			return _eastern.Compute(dateTimeLocal, longitudeDegrees, timeZone);
		}

		public (WesternHoroscopeResult Western, EasternAstrologyResult Eastern) ComputeFull(DateTime dateTimeLocal, double latitudeDegrees, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			DateTime dateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateTimeLocal, timeZone);
			var western = _western.Compute(dateTimeUtc, latitudeDegrees, longitudeDegrees);
			var eastern = _eastern.Compute(dateTimeLocal, longitudeDegrees, timeZone);
			return (western, eastern);
		}
	}
}

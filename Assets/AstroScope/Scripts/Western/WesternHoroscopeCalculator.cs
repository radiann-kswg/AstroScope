using System;
using System.Collections.Generic;

namespace AstroScope
{
	public class WesternHoroscopeCalculator
	{
		private readonly EphemerisCalculator _ephemerisCalculator = new();
		private readonly HouseCalculator _houseCalculator = new();

		public WesternHoroscopeResult Compute(DateTime dateTimeUtc, double latitudeDegrees, double longitudeDegrees)
		{
			var result = new WesternHoroscopeResult
			{
				DateTimeUtc = dateTimeUtc,
				JulianDay = JulianDate.FromDateTime(dateTimeUtc)
			};

			var bodies = _ephemerisCalculator.ComputeAll(dateTimeUtc);
			result.Bodies = new List<CelestialPosition>(bodies);
			result.Houses = _houseCalculator.ComputeEqualHouses(dateTimeUtc, latitudeDegrees, longitudeDegrees);
			return result;
		}
	}
}

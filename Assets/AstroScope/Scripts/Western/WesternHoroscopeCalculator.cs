using System;
using System.Collections.Generic;

namespace AstroScope
{
	/// <summary>
	/// High-level orchestrator for western horoscope computation (planets + houses).
	/// </summary>
	public class WesternHoroscopeCalculator
	{
		private readonly EphemerisCalculator _ephemerisCalculator = new();
		private readonly HouseCalculator _houseCalculator = new();

		/// <summary>Computes a full western horoscope for the given UTC instant and coordinates.</summary>
		public WesternHoroscopeResult Compute(DateTime dateTimeUtc, double latitudeDegrees, double longitudeDegrees)
		{
			var result = new WesternHoroscopeResult
			{
				DateTimeUtc = dateTimeUtc,
				JulianDay = JulianDate.FromDateTime(dateTimeUtc)
			};

			var bodies = _ephemerisCalculator.ComputeAll(dateTimeUtc);
			result.Bodies = new List<CelestialPosition>(bodies);
			result.Placements = new List<ZodiacPlacement>(bodies.Count);
			foreach (var body in bodies)
			{
				result.Placements.Add(new ZodiacPlacement(body.Body, body.Sign, body.DegreesInSign));
			}

			result.Houses = _houseCalculator.ComputeEqualHouses(dateTimeUtc, latitudeDegrees, longitudeDegrees);
			result.MissingCuspSigns = new List<ZodiacSign>(result.Houses.InterceptedSigns);
			return result;
		}
	}
}

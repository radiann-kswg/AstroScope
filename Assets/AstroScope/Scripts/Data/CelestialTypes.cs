using System;
using System.Collections.Generic;
using UnityEngine;

namespace AstroScope
{
	/// <summary>Identifiers for solar system bodies tracked by the ephemeris.</summary>
	public enum PlanetId
	{
		Sun,
		Moon,
		Mercury,
		Venus,
		Mars,
		Jupiter,
		Saturn,
		Uranus,
		Neptune,
		Pluto,
		Earth
	}

	/// <summary>Western zodiac sign enumeration (Aries → Pisces).</summary>
	public enum ZodiacSign
	{
		Aries = 0,
		Taurus = 1,
		Gemini = 2,
		Cancer = 3,
		Leo = 4,
		Virgo = 5,
		Libra = 6,
		Scorpio = 7,
		Sagittarius = 8,
		Capricorn = 9,
		Aquarius = 10,
		Pisces = 11
	}

	/// <summary>Represents a geocentric ecliptic position for a celestial body.</summary>
	[Serializable]
	public struct CelestialPosition
	{
		public PlanetId Body;
		public double EclipticLongitude;
		public double EclipticLatitude;
		public double DistanceAstronomicalUnits;
		public Vector3 EclipticVector;
		public ZodiacSign Sign;
		public double DegreesInSign;

		public CelestialPosition(PlanetId body, double lambda, double beta, double distance, Vector3 eclipticVector, ZodiacSign sign, double degreesInSign)
		{
			Body = body;
			EclipticLongitude = Angle.NormalizeDegrees(lambda);
			EclipticLatitude = beta;
			DistanceAstronomicalUnits = distance;
			EclipticVector = eclipticVector;
			Sign = sign;
			DegreesInSign = degreesInSign;
		}
	}

	/// <summary>Convenience pairing of planet and localized zodiac position.</summary>
	[Serializable]
	public struct ZodiacPlacement
	{
		public PlanetId Body;
		public ZodiacSign Sign;
		public double DegreesInSign;

		public ZodiacPlacement(PlanetId body, ZodiacSign sign, double degreesInSign)
		{
			Body = body;
			Sign = sign;
			DegreesInSign = degreesInSign;
		}
	}

	/// <summary>Stores the longitude of a single Placidus/equal house cusp.</summary>
	[Serializable]
	public struct HousePosition
	{
		public int HouseNumber;
		public double Longitude;

		public HousePosition(int number, double longitude)
		{
			HouseNumber = number;
			Longitude = Angle.NormalizeDegrees(longitude);
		}
	}

	/// <summary>Represents an intercepted sign fully contained within a house.</summary>
	[Serializable]
	public struct HouseIntercept
	{
		public int HouseNumber;
		public ZodiacSign Sign;

		public HouseIntercept(int houseNumber, ZodiacSign sign)
		{
			HouseNumber = houseNumber;
			Sign = sign;
		}
	}

	/// <summary>Contains the computed ascendant, MC, cusps, and intercept data for a horoscope.</summary>
	[Serializable]
	public class HouseCalculationResult
	{
		public double AscendantDegrees { get; set; }
		public double MidheavenDegrees { get; set; }
		public List<HousePosition> Houses { get; set; } = new();
		public List<HouseIntercept> Intercepts { get; set; } = new();
		public Dictionary<int, ZodiacSign> HouseCusps { get; set; } = new();
		public List<ZodiacSign> SignsOnCusps { get; set; } = new();
		public List<ZodiacSign> InterceptedSigns { get; set; } = new();
	}

	/// <summary>Aggregate result for western horoscope computations.</summary>
	[Serializable]
	public class WesternHoroscopeResult
	{
		public DateTime DateTimeUtc { get; set; }
		public double JulianDay { get; set; }
		public List<CelestialPosition> Bodies { get; set; } = new();
		public List<ZodiacPlacement> Placements { get; set; } = new();
		public HouseCalculationResult Houses { get; set; } = new();
		public List<ZodiacSign> MissingCuspSigns { get; set; } = new();
	}
}

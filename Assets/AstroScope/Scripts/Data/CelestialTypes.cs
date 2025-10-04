using System;
using System.Collections.Generic;
using UnityEngine;

namespace AstroScope
{
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

	[Serializable]
	public struct CelestialPosition
	{
		public PlanetId Body;
		public double EclipticLongitude;
		public double EclipticLatitude;
		public double DistanceAstronomicalUnits;
		public Vector3 EclipticVector;

		public CelestialPosition(PlanetId body, double lambda, double beta, double distance, Vector3 eclipticVector)
		{
			Body = body;
			EclipticLongitude = Angle.NormalizeDegrees(lambda);
			EclipticLatitude = beta;
			DistanceAstronomicalUnits = distance;
			EclipticVector = eclipticVector;
		}
	}

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

	[Serializable]
	public class HouseCalculationResult
	{
		public double AscendantDegrees { get; set; }
		public double MidheavenDegrees { get; set; }
		public List<HousePosition> Houses { get; set; } = new();
	}

	[Serializable]
	public class WesternHoroscopeResult
	{
		public DateTime DateTimeUtc { get; set; }
		public double JulianDay { get; set; }
		public List<CelestialPosition> Bodies { get; set; } = new();
		public HouseCalculationResult Houses { get; set; } = new();
	}
}

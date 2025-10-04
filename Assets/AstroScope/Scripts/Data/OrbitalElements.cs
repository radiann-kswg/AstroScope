using System;

namespace AstroScope
{
	/// <summary>
	/// Describes orbital elements and linear rates for VSOP-style heliocentric calculations.
	/// </summary>
	[Serializable]
	public struct OrbitalElements
	{
		public double SemiMajorAxis;
		public double Eccentricity;
		public double Inclination;
		public double LongitudeOfAscendingNode;
		public double LongitudeOfPerihelion;
		public double MeanLongitude;

		public double SemiMajorAxisRate;
		public double EccentricityRate;
		public double InclinationRate;
		public double LongitudeOfAscendingNodeRate;
		public double LongitudeOfPerihelionRate;
		public double MeanLongitudeRate;

		public OrbitalElements(double a, double e, double i, double omega, double perihelion, double meanLongitude,
			double aRate, double eRate, double iRate, double omegaRate, double perihelionRate, double meanLongitudeRate)
		{
			SemiMajorAxis = a;
			Eccentricity = e;
			Inclination = i;
			LongitudeOfAscendingNode = omega;
			LongitudeOfPerihelion = perihelion;
			MeanLongitude = meanLongitude;
			SemiMajorAxisRate = aRate;
			EccentricityRate = eRate;
			InclinationRate = iRate;
			LongitudeOfAscendingNodeRate = omegaRate;
			LongitudeOfPerihelionRate = perihelionRate;
			MeanLongitudeRate = meanLongitudeRate;
		}

		public OrbitalElements ForCenturies(double centuries)
		{
			return new OrbitalElements(
				SemiMajorAxis + SemiMajorAxisRate * centuries,
				Eccentricity + EccentricityRate * centuries,
				Inclination + InclinationRate * centuries,
				LongitudeOfAscendingNode + LongitudeOfAscendingNodeRate * centuries,
				LongitudeOfPerihelion + LongitudeOfPerihelionRate * centuries,
				MeanLongitude + MeanLongitudeRate * centuries,
				SemiMajorAxisRate,
				EccentricityRate,
				InclinationRate,
				LongitudeOfAscendingNodeRate,
				LongitudeOfPerihelionRate,
				MeanLongitudeRate);
		}
	}
}

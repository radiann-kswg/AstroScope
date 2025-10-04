using System;
using System.Collections.Generic;
using UnityEngine;

namespace AstroScope
{
	/// <summary>
	/// Calculates heliocentric and geocentric ephemerides for the Sun, Moon, and major planets.
	/// </summary>
	public class EphemerisCalculator
	{
		private const double AstronomicalUnitKm = 149_597_870.7;

		private static readonly PlanetId[] TrackedPlanets =
		{
			PlanetId.Sun,
			PlanetId.Moon,
			PlanetId.Mercury,
			PlanetId.Venus,
			PlanetId.Mars,
			PlanetId.Jupiter,
			PlanetId.Saturn,
			PlanetId.Uranus,
			PlanetId.Neptune,
			PlanetId.Pluto
		};

		/// <summary>Computes ecliptic positions for all tracked bodies at the supplied UTC instant.</summary>
		public IReadOnlyList<CelestialPosition> ComputeAll(DateTime dateTimeUtc)
		{
			var results = new List<CelestialPosition>(TrackedPlanets.Length);
			foreach (var body in TrackedPlanets)
			{
				results.Add(GetPosition(body, dateTimeUtc));
			}

			return results;
		}

		/// <summary>Retrieves the ecliptic longitude/latitude and distance for a single body.</summary>
		public CelestialPosition GetPosition(PlanetId body, DateTime dateTimeUtc)
		{
			switch (body)
			{
				case PlanetId.Sun:
					return GetSunPosition(dateTimeUtc);
				case PlanetId.Moon:
					return GetMoonPosition(dateTimeUtc);
				case PlanetId.Earth:
					return GetEarthPosition(dateTimeUtc);
				default:
					return GetPlanetPosition(body, dateTimeUtc);
			}
		}

		private CelestialPosition GetSunPosition(DateTime dateTimeUtc)
		{
			var earth = ComputeHeliocentricVector(PlanetId.Earth, dateTimeUtc);
			var sunVector = -earth.Heliocentric;
			double lambda = Angle.NormalizeDegrees(Angle.ToDegrees(Math.Atan2(sunVector.y, sunVector.x)));
			double beta = Angle.ToDegrees(Math.Atan2(sunVector.z, Math.Sqrt(sunVector.x * sunVector.x + sunVector.y * sunVector.y)));
			double distance = sunVector.magnitude;
			return CreatePosition(PlanetId.Sun, lambda, beta, distance, sunVector);
		}

		private CelestialPosition GetEarthPosition(DateTime dateTimeUtc)
		{
			var earth = ComputeHeliocentricVector(PlanetId.Earth, dateTimeUtc);
			double lambda = Angle.NormalizeDegrees(Angle.ToDegrees(Math.Atan2(earth.Heliocentric.y, earth.Heliocentric.x)));
			double beta = Angle.ToDegrees(Math.Atan2(earth.Heliocentric.z, Math.Sqrt(earth.Heliocentric.x * earth.Heliocentric.x + earth.Heliocentric.y * earth.Heliocentric.y)));
			return CreatePosition(PlanetId.Earth, lambda, beta, earth.Distance, earth.Heliocentric);
		}

		private CelestialPosition GetPlanetPosition(PlanetId body, DateTime dateTimeUtc)
		{
			var target = ComputeHeliocentricVector(body, dateTimeUtc);
			var earth = ComputeHeliocentricVector(PlanetId.Earth, dateTimeUtc);
			var geo = target.Heliocentric - earth.Heliocentric;
			double lambda = Angle.NormalizeDegrees(Angle.ToDegrees(Math.Atan2(geo.y, geo.x)));
			double beta = Angle.ToDegrees(Math.Atan2(geo.z, Math.Sqrt(geo.x * geo.x + geo.y * geo.y)));
			double distance = Math.Sqrt(geo.x * geo.x + geo.y * geo.y + geo.z * geo.z);

			return CreatePosition(body, lambda, beta, distance, geo);
		}

		private CelestialPosition GetMoonPosition(DateTime dateTimeUtc)
		{
			double jd = JulianDate.FromDateTime(dateTimeUtc);
			double t = (jd - 2451545.0) / 36525.0;

			double lPrime = Angle.NormalizeDegrees(218.3164477 + 481267.88123421 * t
				- 0.0015786 * t * t + t * t * t / 538841.0 - t * t * t * t / 65194000.0);
			double d = Angle.NormalizeDegrees(297.8501921 + 445267.1114034 * t
				- 0.0018819 * t * t + t * t * t / 545868.0 - t * t * t * t / 113065000.0);
			double m = Angle.NormalizeDegrees(357.5291092 + 35999.0502909 * t
				- 0.0001536 * t * t + t * t * t / 24490000.0);
			double mPrime = Angle.NormalizeDegrees(134.9633964 + 477198.8675055 * t
				+ 0.0087414 * t * t + t * t * t / 69699.0 - t * t * t * t / 14712000.0);
			double f = Angle.NormalizeDegrees(93.2720950 + 483202.0175233 * t
				- 0.0036539 * t * t - t * t * t / 3526000.0 + t * t * t * t / 863310000.0);

			double e = 1.0 - 0.002516 * t - 0.0000074 * t * t;

			double lRad = Angle.ToRadians(lPrime);
			double dRad = Angle.ToRadians(d);
			double mRad = Angle.ToRadians(m);
			double mPrimeRad = Angle.ToRadians(mPrime);
			double fRad = Angle.ToRadians(f);

			double longitude = lPrime
				+ 6.289 * Math.Sin(mPrimeRad)
				+ 1.274 * Math.Sin(2 * dRad - mPrimeRad)
				+ 0.658 * Math.Sin(2 * dRad)
				+ 0.214 * Math.Sin(2 * mPrimeRad)
				- 0.186 * Math.Sin(mRad) * e
				- 0.059 * Math.Sin(2 * dRad - 2 * mPrimeRad)
				- 0.057 * Math.Sin(mPrimeRad - 2 * dRad)
				+ 0.053 * Math.Sin(mPrimeRad + 2 * dRad)
				+ 0.046 * Math.Sin(2 * dRad - mRad) * e
				+ 0.041 * Math.Sin(mPrimeRad - mRad) * e
				- 0.035 * Math.Sin(dRad)
				- 0.031 * Math.Sin(mPrimeRad + mRad) * e
				- 0.015 * Math.Sin(2 * fRad - 2 * dRad)
				+ 0.011 * Math.Sin(mPrimeRad - 4 * dRad);

			double latitude = 5.128 * Math.Sin(fRad)
				+ 0.280 * Math.Sin(mPrimeRad + fRad)
				+ 0.277 * Math.Sin(mPrimeRad - fRad)
				+ 0.173 * Math.Sin(2 * dRad - fRad)
				+ 0.055 * Math.Sin(2 * dRad + fRad - mPrimeRad)
				+ 0.046 * Math.Sin(2 * dRad - fRad - mPrimeRad)
				+ 0.033 * Math.Sin(2 * dRad + fRad)
				+ 0.017 * Math.Sin(2 * mPrimeRad + fRad)
				+ 0.009 * Math.Sin(2 * dRad + mPrimeRad - fRad)
				+ 0.009 * Math.Sin(2 * dRad - mPrimeRad - fRad);

			double distanceKm = 385000.56
				- 20905.355 * Math.Cos(mPrimeRad)
				- 3699.111 * Math.Cos(2 * dRad - mPrimeRad)
				- 2955.968 * Math.Cos(2 * dRad)
				- 569.925 * Math.Cos(2 * mPrimeRad)
				+ 48.888 * Math.Cos(2 * dRad - 2 * mPrimeRad)
				- 3.148 * Math.Cos(2 * dRad + mPrimeRad);

			double distanceAu = distanceKm / AstronomicalUnitKm;
			double lambdaRad = Angle.ToRadians(longitude);
			double betaRad = Angle.ToRadians(latitude);
			double cosBeta = Math.Cos(betaRad);
			double x = distanceAu * cosBeta * Math.Cos(lambdaRad);
			double y = distanceAu * cosBeta * Math.Sin(lambdaRad);
			double z = distanceAu * Math.Sin(betaRad);

			return CreatePosition(
				PlanetId.Moon,
				Angle.NormalizeDegrees(longitude),
				latitude,
				distanceAu,
				new Vector3((float)x, (float)y, (float)z));
		}

		private static CelestialPosition CreatePosition(PlanetId body, double lambda, double beta, double distance, Vector3 vector)
		{
			double normalizedLongitude = Angle.NormalizeDegrees(lambda);
			var sign = ZodiacUtility.GetSign(normalizedLongitude);
			double degreesInSign = ZodiacUtility.GetDegreesInSign(normalizedLongitude);
			return new CelestialPosition(body, normalizedLongitude, beta, distance, vector, sign, degreesInSign);
		}

		private (Vector3 Heliocentric, double Distance) ComputeHeliocentricVector(PlanetId planet, DateTime dateTimeUtc)
		{
			double jd = JulianDate.FromDateTime(dateTimeUtc);
			double t = JulianDate.ToJulianCenturies(jd);

			var kepler = OrbitalElementsDatabase.GetElements(planet).ForCenturies(t);

			double a = kepler.SemiMajorAxis;
			double e = kepler.Eccentricity;
			double i = Angle.ToRadians(kepler.Inclination);
			double omega = Angle.ToRadians(kepler.LongitudeOfAscendingNode);
			double pi = Angle.ToRadians(kepler.LongitudeOfPerihelion);
			double l = Angle.ToRadians(kepler.MeanLongitude);
			double w = pi - omega;
			double m = Angle.NormalizeRadians(l - pi);

			double eAnomaly = SolveKepler(e, m);
			double xv = a * (Math.Cos(eAnomaly) - e);
			double yv = a * Math.Sqrt(1.0 - e * e) * Math.Sin(eAnomaly);

			double v = Math.Atan2(yv, xv);
			double r = Math.Sqrt(xv * xv + yv * yv);

			double cosOmega = Math.Cos(omega);
			double sinOmega = Math.Sin(omega);
			double cosI = Math.Cos(i);
			double sinI = Math.Sin(i);
			double cosWv = Math.Cos(v + w);
			double sinWv = Math.Sin(v + w);

			double xh = r * (cosOmega * cosWv - sinOmega * sinWv * cosI);
			double yh = r * (sinOmega * cosWv + cosOmega * sinWv * cosI);
			double zh = r * (sinWv * sinI);

			return (new Vector3((float)xh, (float)yh, (float)zh), r);
		}

		private static double SolveKepler(double eccentricity, double meanAnomaly)
		{
			double m = meanAnomaly;
			double e = eccentricity;
			double eAnomaly = m;
			double delta;
			int iteration = 0;
			const double tolerance = 1e-10;

			do
			{
				double f = eAnomaly - e * Math.Sin(eAnomaly) - m;
				double fPrime = 1.0 - e * Math.Cos(eAnomaly);
				delta = f / fPrime;
				eAnomaly -= delta;
				iteration++;
			}
			while (Math.Abs(delta) > tolerance && iteration < 50);

			return eAnomaly;
		}


	}
}

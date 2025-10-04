using System;

namespace AstroScope
{
	/// <summary>
	/// Astronomical time helpers (obliquity and sidereal time calculations).
	/// </summary>
	public static class AstroTime
	{
		/// <summary>Computes the mean obliquity of the ecliptic (ε) for the specified Julian centuries since J2000.</summary>
		public static double MeanObliquity(double julianCenturies)
		{
			double seconds = 21.448 - julianCenturies * (46.815 + julianCenturies * (0.00059 - julianCenturies * 0.001813));
			double degrees = 23.0 + (26.0 + seconds / 60.0) / 60.0;
			return degrees;
		}

		/// <summary>Computes the apparent local sidereal time in degrees for a given UTC moment and observer longitude.</summary>
		public static double ApparentSiderealTime(DateTime dateTimeUtc, double longitude)
		{
			double jd = JulianDate.FromDateTime(dateTimeUtc);
			double t = JulianDate.ToJulianCenturies(jd);
			double theta0 = 280.46061837 + 360.98564736629 * (jd - JulianDate.J2000)
				+ t * t * (0.000387933 - t / 38710000.0);

			double gmst = Angle.NormalizeDegrees(theta0);
			double lmst = Angle.NormalizeDegrees(gmst + longitude);

			return lmst;
		}

		/// <summary>Computes the mean Greenwich sidereal time in degrees for a UTC instant.</summary>
		public static double MeanSiderealTime(DateTime dateTimeUtc)
		{
			double jd = JulianDate.FromDateTime(dateTimeUtc);
			double d = JulianDate.DaysSinceJ2000(jd);
			double t = d / 36525.0;
			double gmst = 280.46061837 + 360.98564736629 * d + 0.000387933 * t * t - t * t * t / 38710000.0;
			return Angle.NormalizeDegrees(gmst);
		}
	}
}

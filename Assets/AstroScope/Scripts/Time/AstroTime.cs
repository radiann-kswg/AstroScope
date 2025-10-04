using System;

namespace AstroScope
{
	public static class AstroTime
	{
		public static double MeanObliquity(double julianCenturies)
		{
			double seconds = 21.448 - julianCenturies * (46.815 + julianCenturies * (0.00059 - julianCenturies * 0.001813));
			double degrees = 23.0 + (26.0 + seconds / 60.0) / 60.0;
			return degrees;
		}

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

using System;

namespace AstroScope
{
	/// <summary>
	/// Provides utility helpers for converting and normalizing angular values.
	/// </summary>
	public static class Angle
	{
		/// <summary>Full turn in radians.</summary>
		public const double TwoPi = Math.PI * 2.0;

		/// <summary>Conversion factor from radians to degrees.</summary>
		public const double DegreesPerRadian = 180.0 / Math.PI;

		/// <summary>Conversion factor from degrees to radians.</summary>
		public const double RadiansPerDegree = Math.PI / 180.0;

		/// <summary>Converts degrees to radians.</summary>
		public static double ToRadians(double degrees)
		{
			return degrees * RadiansPerDegree;
		}

		/// <summary>Converts radians to degrees.</summary>
		public static double ToDegrees(double radians)
		{
			return radians * DegreesPerRadian;
		}

		/// <summary>Normalizes a radian value into the range [0, 2π).</summary>
		public static double NormalizeRadians(double radians)
		{
			double value = radians % TwoPi;
			return value < 0 ? value + TwoPi : value;
		}

		/// <summary>Normalizes a degree value into the range [0°, 360°).</summary>
		public static double NormalizeDegrees(double degrees)
		{
			double value = degrees % 360.0;
			return value < 0 ? value + 360.0 : value;
		}

		/// <summary>Wraps a degree value into the signed range (-180°, 180°].</summary>
		public static double WrapDegrees180(double degrees)
		{
			double value = NormalizeDegrees(degrees);
			return value > 180.0 ? value - 360.0 : value;
		}

		/// <summary>Converts degrees to hours (15° per hour).</summary>
		public static double ToHours(double degrees)
		{
			return degrees / 15.0;
		}

		/// <summary>Converts hours (sidereal) back to degrees.</summary>
		public static double FromHours(double hours)
		{
			return hours * 15.0;
		}
	}
}

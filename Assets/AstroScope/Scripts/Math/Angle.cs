using System;

namespace AstroScope
{
	public static class Angle
	{
		public const double TwoPi = Math.PI * 2.0;
		public const double DegreesPerRadian = 180.0 / Math.PI;
		public const double RadiansPerDegree = Math.PI / 180.0;

		public static double ToRadians(double degrees)
		{
			return degrees * RadiansPerDegree;
		}

		public static double ToDegrees(double radians)
		{
			return radians * DegreesPerRadian;
		}

		public static double NormalizeRadians(double radians)
		{
			double value = radians % TwoPi;
			return value < 0 ? value + TwoPi : value;
		}

		public static double NormalizeDegrees(double degrees)
		{
			double value = degrees % 360.0;
			return value < 0 ? value + 360.0 : value;
		}

		public static double WrapDegrees180(double degrees)
		{
			double value = NormalizeDegrees(degrees);
			return value > 180.0 ? value - 360.0 : value;
		}

		public static double ToHours(double degrees)
		{
			return degrees / 15.0;
		}

		public static double FromHours(double hours)
		{
			return hours * 15.0;
		}
	}
}

using System;

namespace AstroScope
{
	public static class ZodiacUtility
	{
		private const double DegreesPerSign = 30.0;
		private const int SignCount = 12;

		public static ZodiacSign GetSign(double eclipticLongitude)
		{
			double normalized = Angle.NormalizeDegrees(eclipticLongitude);
			int index = (int)Math.Floor(normalized / DegreesPerSign);
			if (index >= SignCount)
			{
				index = SignCount - 1;
			}
			return (ZodiacSign)index;
		}

		public static double GetDegreesInSign(double eclipticLongitude)
		{
			double normalized = Angle.NormalizeDegrees(eclipticLongitude);
			double remainder = normalized % DegreesPerSign;
			return remainder;
		}

		public static double GetSignStartLongitude(ZodiacSign sign)
		{
			return (int)sign * DegreesPerSign;
		}

		public static double GetSignEndLongitude(ZodiacSign sign)
		{
			return GetSignStartLongitude(sign) + DegreesPerSign;
		}
	}
}

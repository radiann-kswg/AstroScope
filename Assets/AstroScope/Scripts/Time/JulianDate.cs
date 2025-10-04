using System;

namespace AstroScope
{
	/// <summary>
	/// Utility conversions between <see cref="DateTime"/> and astronomical Julian day numbers.
	/// </summary>
	public static class JulianDate
	{
		/// <summary>Julian day number at the J2000.0 epoch (2000-01-01 12:00 TT).</summary>
		public const double J2000 = 2451545.0;

		/// <summary>Number of days in a Julian century.</summary>
		public const double JulianCentury = 36525.0;

		/// <summary>Number of seconds in a Julian day.</summary>
		public const double JulianDay = 86400.0;

		/// <summary>Converts a UTC <see cref="DateTime"/> to a Julian day number.</summary>
		public static double FromDateTime(DateTime dateTimeUtc)
		{
			if (dateTimeUtc.Kind != DateTimeKind.Utc)
			{
				dateTimeUtc = DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc);
			}

			int year = dateTimeUtc.Year;
			int month = dateTimeUtc.Month;
			double day = dateTimeUtc.Day +
				(dateTimeUtc.Hour +
				(dateTimeUtc.Minute +
				(dateTimeUtc.Second + dateTimeUtc.Millisecond / 1000.0) / 60.0) / 60.0) / 24.0;

			if (month <= 2)
			{
				year -= 1;
				month += 12;
			}

			int a = year / 100;
			int b = 2 - a + (a / 4);

			double jd = Math.Floor(365.25 * (year + 4716))
				+ Math.Floor(30.6001 * (month + 1))
				+ day + b - 1524.5;

			return jd;
		}

		/// <summary>Converts a Julian day number into centuries elapsed since J2000.0.</summary>
		public static double ToJulianCenturies(double julianDay)
		{
			return (julianDay - J2000) / JulianCentury;
		}

		/// <summary>Returns the number of days elapsed since the J2000.0 epoch.</summary>
		public static double DaysSinceJ2000(double julianDay)
		{
			return julianDay - J2000;
		}

		/// <summary>Converts a Julian day number back into a UTC <see cref="DateTime"/>.</summary>
		public static DateTime ToDateTime(double julianDay)
		{
			double jd = julianDay + 0.5;
			int z = (int)Math.Floor(jd);
			double f = jd - z;

			int a = z;
			if (z >= 2299161)
			{
				int alpha = (int)((z - 1867216.25) / 36524.25);
				a = z + 1 + alpha - alpha / 4;
			}

			int b = a + 1524;
			int c = (int)((b - 122.1) / 365.25);
			int d = (int)(365.25 * c);
			int e = (int)((b - d) / 30.6001);

			double day = b - d - Math.Floor(30.6001 * e) + f;
			int month = e < 14 ? e - 1 : e - 13;
			int year = month > 2 ? c - 4716 : c - 4715;

			int dayInt = (int)Math.Floor(day);
			double fractionalDay = day - dayInt;
			double hour = fractionalDay * 24.0;
			int hourInt = (int)Math.Floor(hour);
			double minute = (hour - hourInt) * 60.0;
			int minuteInt = (int)Math.Floor(minute);
			double second = (minute - minuteInt) * 60.0;

			return new DateTime(year, month, dayInt, hourInt, minuteInt, (int)Math.Floor(second), DateTimeKind.Utc)
				.AddMilliseconds((second - Math.Floor(second)) * 1000.0);
		}
	}
}

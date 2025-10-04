using System;

namespace AstroScope
{
	public class EasternAstrologyCalculator
	{
		private const double SynodicMonth = 29.530588853;
		private const double NewMoonEpochJd = 2451550.09765; // 2000-01-06 18:14 UT

		private readonly EphemerisCalculator _ephemerisCalculator = new();

		public EasternAstrologyResult Compute(DateTime dateTimeLocal, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			DateTime dateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateTimeLocal, timeZone);
			double julianDay = JulianDate.FromDateTime(dateTimeUtc);
			double lunarAge = ComputeLunarAgeDays(julianDay);
			var (prevNewMoon, nextNewMoon) = GetBoundingNewMoons(julianDay);
			var lunisolarDate = BuildLunisolarDate(julianDay, prevNewMoon, nextNewMoon);

			DateTime setsuLocal = FindSetsubun(dateTimeLocal.Year, longitudeDegrees, timeZone);
			if (dateTimeLocal < setsuLocal)
			{
				setsuLocal = FindSetsubun(dateTimeLocal.Year - 1, longitudeDegrees, timeZone);
			}

			var setsuUtc = TimeZoneInfo.ConvertTimeToUtc(setsuLocal, timeZone);
			int referenceYear = dateTimeLocal >= setsuLocal
				? dateTimeLocal.Year
				: dateTimeLocal.Year - 1;

			int yearStar = ComputeNineStarYear(referenceYear);
			int monthStar = ComputeNineStarMonth(yearStar, lunisolarDate.Month);
			int dayStar = ComputeNineStarDay(julianDay);

			return new EasternAstrologyResult
			{
				DateTimeUtc = dateTimeUtc,
				LunisolarDate = lunisolarDate,
				LunarAgeDays = lunarAge,
				SetsubunLocal = setsuLocal,
				SetsubunUtc = setsuUtc,
				Chart = new NineStarKiChart
				{
					YearStar = yearStar,
					MonthStar = monthStar,
					DayStar = dayStar
				}
			};
		}

		private double ComputeLunarAgeDays(double julianDay)
		{
			var (prevNewMoon, _) = GetBoundingNewMoons(julianDay);
			return julianDay - prevNewMoon;
		}

		private (double Prev, double Next) GetBoundingNewMoons(double julianDay)
		{
			double k = Math.Floor((julianDay - NewMoonEpochJd) / SynodicMonth);
			double prev = TrueNewMoon(k);
			double next = TrueNewMoon(k + 1);
			if (prev > julianDay)
			{
				k -= 1;
				prev = TrueNewMoon(k);
			}
			if (next < julianDay)
			{
				next = TrueNewMoon(k + 2);
			}
			return (prev, next);
		}

		private LunisolarDate BuildLunisolarDate(double julianDay, double prevNewMoon, double nextNewMoon)
		{
			int day = (int)Math.Floor(julianDay - prevNewMoon) + 1;
			int monthIndex = ComputeLunisolarMonthIndex(julianDay, prevNewMoon, nextNewMoon);

			bool hasPrincipalTerm = HasPrincipalTerm(prevNewMoon, nextNewMoon);

			var prevDate = JulianDate.ToDateTime(prevNewMoon);
			int lunisolarYear = prevDate.Month >= 11 ? prevDate.Year + 1 : prevDate.Year;

			return new LunisolarDate
			{
				Year = lunisolarYear,
				Month = monthIndex,
				Day = day,
				IsLeapMonth = !hasPrincipalTerm
			};
		}

		private int ComputeLunisolarMonthIndex(double julianDay, double prevNewMoon, double nextNewMoon)
		{
			double midPoint = (prevNewMoon + nextNewMoon) * 0.5;
			DateTime midDate = JulianDate.ToDateTime(midPoint);
			double sunLongitude = _ephemerisCalculator.GetPosition(PlanetId.Sun, midDate).EclipticLongitude;
			int segment = (int)Math.Floor(Angle.NormalizeDegrees(sunLongitude) / 30.0);
			int month = ((segment + 1) % 12) + 1;

			if (!HasPrincipalTerm(prevNewMoon, nextNewMoon))
			{
				return month;
			}

			return month;
		}

		private bool HasPrincipalTerm(double startJd, double endJd)
		{
			double startLon = _ephemerisCalculator.GetPosition(PlanetId.Sun, JulianDate.ToDateTime(startJd + 0.5)).EclipticLongitude;
			double endLon = _ephemerisCalculator.GetPosition(PlanetId.Sun, JulianDate.ToDateTime(endJd - 0.5)).EclipticLongitude;
			int startSegment = (int)Math.Floor(Angle.NormalizeDegrees(startLon) / 30.0);
			int endSegment = (int)Math.Floor(Angle.NormalizeDegrees(endLon) / 30.0);
			return startSegment != endSegment;
		}

		private DateTime FindSetsubun(int year, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			DateTime approxLocal = new DateTime(year, 2, 3, 0, 0, 0, DateTimeKind.Unspecified);
			TimeSpan tzOffset = timeZone.GetUtcOffset(approxLocal);
			double solarTimeHours = longitudeDegrees / 15.0;
			double civilTimeHours = tzOffset.TotalHours;
			double adjustment = solarTimeHours - civilTimeHours;
			approxLocal = approxLocal.AddHours(adjustment);
			approxLocal = TimeZoneInfo.ConvertTimeToUtc(approxLocal, timeZone);
			DateTime start = approxLocal.AddDays(-1);
			DateTime end = approxLocal.AddDays(2);

			double target = 315.0;
			DateTime best = approxLocal;
			double bestDiff = double.MaxValue;
			TimeSpan step = TimeSpan.FromHours(6);
			DateTime current = start;
			double prevDiff = Angle.WrapDegrees180(_ephemerisCalculator.GetPosition(PlanetId.Sun, current).EclipticLongitude - target);
			while (current <= end)
			{
				DateTime next = current.Add(step);
				if (next > end)
				{
					next = end;
				}
				double diff = Angle.WrapDegrees180(_ephemerisCalculator.GetPosition(PlanetId.Sun, next).EclipticLongitude - target);
				if (prevDiff == 0 || prevDiff * diff <= 0)
				{
					best = RefineSolarLongitudeCrossing(current, next, target);
					break;
				}

				if (Math.Abs(diff) < bestDiff)
				{
					bestDiff = Math.Abs(diff);
					best = next;
				}

				current = next;
				prevDiff = diff;
			}

			return TimeZoneInfo.ConvertTimeFromUtc(best, timeZone);
		}

		private DateTime RefineSolarLongitudeCrossing(DateTime start, DateTime end, double targetLongitude)
		{
			DateTime low = start;
			DateTime high = end;
			for (int i = 0; i < 20; i++)
			{
				DateTime mid = low + TimeSpan.FromTicks((high.Ticks - low.Ticks) / 2);
				double diff = Angle.WrapDegrees180(_ephemerisCalculator.GetPosition(PlanetId.Sun, mid).EclipticLongitude - targetLongitude);
				if (Math.Abs(diff) < 0.0001)
				{
					return mid;
				}

				double diffLow = Angle.WrapDegrees180(_ephemerisCalculator.GetPosition(PlanetId.Sun, low).EclipticLongitude - targetLongitude);
				if (diffLow * diff <= 0)
				{
					high = mid;
				}
				else
				{
					low = mid;
				}
			}

			return low + TimeSpan.FromTicks((high.Ticks - low.Ticks) / 2);
		}

		private int ComputeNineStarYear(int referenceYear)
		{
			int mod = referenceYear % 9;
			if (mod < 0)
			{
				mod += 9;
			}

			int star = 11 - mod;
			while (star > 9)
			{
				star -= 9;
			}
			while (star <= 0)
			{
				star += 9;
			}
			return star;
		}

		private int ComputeNineStarMonth(int yearStar, int monthIndex)
		{
			int star = (yearStar + monthIndex + 6) % 9;
			return star == 0 ? 9 : star;
		}

		private int ComputeNineStarDay(double julianDay)
		{
			int dayIndex = (int)Math.Floor(julianDay + 1.5);
			int star = (dayIndex + 8) % 9 + 1;
			return star;
		}

		private double TrueNewMoon(double k)
		{
			double t = k / 1236.85;
			double t2 = t * t;
			double t3 = t2 * t;
			double t4 = t3 * t;

			double jde = NewMoonEpochJd + SynodicMonth * k + 0.0001337 * t2 - 0.000000150 * t3 + 0.00000000073 * t4;

			double e = 1 - 0.002516 * t - 0.0000074 * t2;
			double m = Angle.ToRadians(2.5534 + 29.10535670 * k - 0.0000014 * t2 - 0.00000011 * t3);
			double mPrime = Angle.ToRadians(201.5643 + 385.81693528 * k + 0.0107582 * t2 + 0.00001238 * t3 - 0.000000058 * t4);
			double f = Angle.ToRadians(160.7108 + 390.67050284 * k - 0.0016118 * t2 - 0.00000227 * t3 + 0.000000011 * t4);
			double omega = Angle.ToRadians(124.7746 - 1.56375580 * k + 0.0020691 * t2 + 0.00000215 * t3);

			double deltaJd = -0.40720 * Math.Sin(mPrime)
				+ 0.17241 * e * Math.Sin(m)
				+ 0.01608 * Math.Sin(2 * mPrime)
				+ 0.01039 * Math.Sin(2 * f)
				+ 0.00739 * e * Math.Sin(mPrime - m)
				- 0.00514 * e * Math.Sin(mPrime + m)
				+ 0.00208 * e * e * Math.Sin(2 * m)
				- 0.00111 * Math.Sin(mPrime - 2 * f)
				- 0.00057 * Math.Sin(mPrime + 2 * f)
				+ 0.00056 * e * Math.Sin(2 * mPrime + m)
				- 0.00042 * Math.Sin(3 * mPrime)
				+ 0.00042 * e * Math.Sin(m + 2 * f)
				+ 0.00038 * e * Math.Sin(m - 2 * f)
				- 0.00024 * e * Math.Sin(2 * mPrime - m)
				- 0.00017 * Math.Sin(omega);

			deltaJd += -0.00004 * Math.Sin(mPrime + m + 2 * f)
				+ 0.00004 * Math.Sin(2 * mPrime - 2 * f)
				+ 0.00003 * Math.Sin(mPrime + m - 2 * f)
				+ 0.00003 * Math.Sin(2 * mPrime + 2 * f)
				+ 0.00002 * Math.Sin(mPrime - m - 2 * f)
				- 0.00002 * Math.Sin(3 * mPrime + m)
				+ 0.00002 * Math.Sin(4 * mPrime);

			double a1 = Angle.ToRadians(299.77 + 0.107408 * k - 0.009173 * t2);
			double a2 = Angle.ToRadians(251.88 + 0.016321 * k);
			double a3 = Angle.ToRadians(251.83 + 26.651886 * k);

			deltaJd += 0.000325 * Math.Sin(a1) + 0.000165 * Math.Sin(a2) + 0.000164 * Math.Sin(a3);

			return jde + deltaJd;
		}
	}
}

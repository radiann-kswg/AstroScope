using System;
using System.Collections.Generic;

namespace AstroScope
{
	/// <summary>
	/// Calculates lunisolar calendar data, Nine Star Ki, sexagenary cycles, solar terms, and doyō periods.
	/// </summary>
	public class EasternAstrologyCalculator
	{
		private const double SynodicMonth = 29.530588853;
		private const double NewMoonEpochJd = 2451550.09765; // 2000-01-06 18:14 UT
		private static readonly (SolarTermId Id, double Longitude, int Month, int Day)[] SolarTermDefinitions =
		{
			(SolarTermId.MinorCold, 285.0, 1, 5),
			(SolarTermId.MajorCold, 300.0, 1, 20),
			(SolarTermId.StartOfSpring, 315.0, 2, 4),
			(SolarTermId.RainWater, 330.0, 2, 19),
			(SolarTermId.AwakeningOfInsects, 345.0, 3, 6),
			(SolarTermId.SpringEquinox, 0.0, 3, 21),
			(SolarTermId.PureBrightness, 15.0, 4, 5),
			(SolarTermId.GrainRain, 30.0, 4, 20),
			(SolarTermId.StartOfSummer, 45.0, 5, 5),
			(SolarTermId.GrainFull, 60.0, 5, 20),
			(SolarTermId.GrainInEar, 75.0, 6, 5),
			(SolarTermId.SummerSolstice, 90.0, 6, 21),
			(SolarTermId.MinorHeat, 105.0, 7, 7),
			(SolarTermId.MajorHeat, 120.0, 7, 23),
			(SolarTermId.StartOfAutumn, 135.0, 8, 7),
			(SolarTermId.EndOfHeat, 150.0, 8, 23),
			(SolarTermId.WhiteDew, 165.0, 9, 7),
			(SolarTermId.AutumnEquinox, 180.0, 9, 23),
			(SolarTermId.ColdDew, 195.0, 10, 8),
			(SolarTermId.FrostDescent, 210.0, 10, 23),
			(SolarTermId.StartOfWinter, 225.0, 11, 7),
			(SolarTermId.MinorSnow, 240.0, 11, 22),
			(SolarTermId.MajorSnow, 255.0, 12, 7),
			(SolarTermId.WinterSolstice, 270.0, 12, 21)
		};

		private readonly EphemerisCalculator _ephemerisCalculator = new();

		/// <summary>
		/// Computes Eastern astrology measurements for a local civil time, including lunisolar date,
		/// 九星, 干支, 二十四節気, and 土用 periods. Local time zone is required for conversions.
		/// </summary>
		public EasternAstrologyResult Compute(DateTime dateTimeLocal, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			DateTime dateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(dateTimeLocal, timeZone);
			double julianDay = JulianDate.FromDateTime(dateTimeUtc);
			double lunarAge = ComputeLunarAgeDays(julianDay);
			var (prevNewMoon, nextNewMoon) = GetBoundingNewMoons(julianDay);
			var lunisolarDate = BuildLunisolarDate(julianDay, prevNewMoon, nextNewMoon);

			var solarTermCache = BuildSolarTermCache(dateTimeLocal.Year, longitudeDegrees, timeZone);
			var currentLichun = solarTermCache[(dateTimeLocal.Year, SolarTermId.StartOfSpring)];
			var previousLichun = solarTermCache[(dateTimeLocal.Year - 1, SolarTermId.StartOfSpring)];

			bool afterCurrentLichun = dateTimeLocal >= currentLichun.DateTimeLocal;
			var activeLichun = afterCurrentLichun ? currentLichun : previousLichun;

			DateTime setsuLocal = activeLichun.DateTimeLocal;
			DateTime setsuUtc = activeLichun.DateTimeUtc;
			int referenceYear = afterCurrentLichun ? dateTimeLocal.Year : dateTimeLocal.Year - 1;

			int yearStar = ComputeNineStarYear(referenceYear);
			int monthStar = ComputeNineStarMonth(yearStar, lunisolarDate.Month);
			int dayStar = ComputeNineStarDay(julianDay);

			var sexagenary = ComputeSexagenary(referenceYear, lunisolarDate.Month, julianDay);
			var solarTermsForYear = GetSolarTermsForYear(dateTimeLocal.Year, solarTermCache);
			var doyouPeriods = ComputeDoyouPeriods(dateTimeLocal.Year, solarTermCache, timeZone);

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
				},
				Sexagenary = sexagenary,
				SolarTerms = solarTermsForYear,
				DoyouPeriods = doyouPeriods
			};
		}

		private Dictionary<(int Year, SolarTermId Id), SolarTermEntry> BuildSolarTermCache(int centerYear, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			var cache = new Dictionary<(int, SolarTermId), SolarTermEntry>();
			AddSolarTermsForYear(cache, centerYear - 1, longitudeDegrees, timeZone);
			AddSolarTermsForYear(cache, centerYear, longitudeDegrees, timeZone);
			AddSolarTermsForYear(cache, centerYear + 1, longitudeDegrees, timeZone);
			return cache;
		}

		private void AddSolarTermsForYear(Dictionary<(int, SolarTermId), SolarTermEntry> cache, int year, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			foreach (var definition in SolarTermDefinitions)
			{
				cache[(year, definition.Id)] = ComputeSolarTermEntry(year, definition, longitudeDegrees, timeZone);
			}
		}

		private SolarTermEntry ComputeSolarTermEntry(int year, (SolarTermId Id, double Longitude, int Month, int Day) definition, double longitudeDegrees, TimeZoneInfo timeZone)
		{
			TimeSpan longitudeOffset = TimeSpan.FromHours(longitudeDegrees / 15.0);
			DateTime approximateUtc = new DateTime(year, definition.Month, definition.Day, 0, 0, 0, DateTimeKind.Utc) - longitudeOffset;
			DateTime start = approximateUtc.AddDays(-7);
			DateTime end = approximateUtc.AddDays(7);
			DateTime crossingUtc = FindSolarLongitudeCrossingUtc(start, end, definition.Longitude);
			return new SolarTermEntry
			{
				Id = definition.Id,
				TargetLongitude = definition.Longitude,
				DateTimeUtc = crossingUtc,
				DateTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(crossingUtc, timeZone)
			};
		}

		private static List<SolarTermEntry> GetSolarTermsForYear(int year, Dictionary<(int, SolarTermId), SolarTermEntry> cache)
		{
			var list = new List<SolarTermEntry>(SolarTermDefinitions.Length);
			foreach (var definition in SolarTermDefinitions)
			{
				if (cache.TryGetValue((year, definition.Id), out var entry))
				{
					list.Add(entry);
				}
			}
			list.Sort((a, b) => a.DateTimeUtc.CompareTo(b.DateTimeUtc));
			return list;
		}

		private List<DoyouPeriod> ComputeDoyouPeriods(int year, Dictionary<(int, SolarTermId), SolarTermEntry> cache, TimeZoneInfo timeZone)
		{
			var list = new List<DoyouPeriod>(4)
			{
				CreateDoyouPeriod(DoyouSeason.Spring, cache[(year, SolarTermId.StartOfSpring)], timeZone),
				CreateDoyouPeriod(DoyouSeason.Summer, cache[(year, SolarTermId.StartOfSummer)], timeZone),
				CreateDoyouPeriod(DoyouSeason.Autumn, cache[(year, SolarTermId.StartOfAutumn)], timeZone),
				CreateDoyouPeriod(DoyouSeason.Winter, cache[(year + 1, SolarTermId.StartOfSpring)], timeZone)
			};
			return list;
		}

		private static DoyouPeriod CreateDoyouPeriod(DoyouSeason season, SolarTermEntry anchor, TimeZoneInfo timeZone)
		{
			DateTime startUtc = anchor.DateTimeUtc.AddDays(-18);
			DateTime endUtc = anchor.DateTimeUtc;
			return new DoyouPeriod
			{
				Season = season,
				StartUtc = startUtc,
				EndUtc = endUtc,
				StartLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone),
				EndLocal = anchor.DateTimeLocal
			};
		}

		private static SexagenaryDate ComputeSexagenary(int referenceYear, int lunisolarMonth, double julianDay)
		{
			int yearStemIndex = Mod(referenceYear - 4, 10);
			int yearBranchIndex = Mod(referenceYear - 4, 12);

			int monthIndex = lunisolarMonth <= 0 ? 1 : lunisolarMonth;
			int monthStemIndex = Mod(yearStemIndex * 2 + monthIndex - 1, 10);
			int monthBranchIndex = Mod(monthIndex + 1, 12);

			int julianDayNumber = (int)Math.Floor(julianDay + 0.5);
			int dayStemIndex = Mod(julianDayNumber + 9, 10);
			int dayBranchIndex = Mod(julianDayNumber + 1, 12);

			return new SexagenaryDate
			{
				Year = new SexagenaryCycle
				{
					Stem = (HeavenlyStem)yearStemIndex,
					Branch = (EarthlyBranch)yearBranchIndex
				},
				Month = new SexagenaryCycle
				{
					Stem = (HeavenlyStem)monthStemIndex,
					Branch = (EarthlyBranch)monthBranchIndex
				},
				Day = new SexagenaryCycle
				{
					Stem = (HeavenlyStem)dayStemIndex,
					Branch = (EarthlyBranch)dayBranchIndex
				}
			};
		}

		private DateTime FindSolarLongitudeCrossingUtc(DateTime startUtc, DateTime endUtc, double targetLongitude)
		{
			const int maxIterations = 1024;
			TimeSpan step = TimeSpan.FromHours(6);
			DateTime current = startUtc;
			double previousDiff = LongitudeDifference(current, targetLongitude);
			for (int i = 0; i < maxIterations && current < endUtc; i++)
			{
				DateTime next = current.Add(step);
				if (next > endUtc)
				{
					next = endUtc;
				}

				double nextDiff = LongitudeDifference(next, targetLongitude);
				if (previousDiff == 0.0 || previousDiff * nextDiff <= 0.0)
				{
					return RefineSolarLongitudeCrossingUtc(current, next, targetLongitude);
				}

				current = next;
				previousDiff = nextDiff;
			}

			return RefineSolarLongitudeCrossingUtc(current, endUtc, targetLongitude);
		}

		private DateTime RefineSolarLongitudeCrossingUtc(DateTime low, DateTime high, double targetLongitude)
		{
			for (int i = 0; i < 32; i++)
			{
				DateTime mid = low + TimeSpan.FromTicks((high.Ticks - low.Ticks) / 2);
				double diff = LongitudeDifference(mid, targetLongitude);
				if (Math.Abs(diff) < 1e-6)
				{
					return mid;
				}

				double diffLow = LongitudeDifference(low, targetLongitude);
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

		private double LongitudeDifference(DateTime utc, double targetLongitude)
		{
			double longitude = _ephemerisCalculator.GetPosition(PlanetId.Sun, utc).EclipticLongitude;
			return Angle.WrapDegrees180(longitude - targetLongitude);
		}

		private static int Mod(int value, int modulus)
		{
			int result = value % modulus;
			return result < 0 ? result + modulus : result;
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

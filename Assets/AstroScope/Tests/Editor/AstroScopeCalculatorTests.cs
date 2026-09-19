// Excluded from player builds: NUnit is unavailable outside the editor/test
// context, and including this file in Assembly-CSharp breaks WebGL builds
// (UnityLinker error IL1005: failed to resolve 'nunit.framework').
#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;

namespace AstroScope.Tests
{
	public class AstroScopeCalculatorTests
	{
		// Windows の Mono は IANA 名 ("Asia/Tokyo") を解決できないので、固定オフセットで作る（Mac/Windows 共通）。
		private static readonly TimeZoneInfo Jst = TimeZoneInfo.CreateCustomTimeZone("JST", TimeSpan.FromHours(9), "JST", "JST");

		[Test]
		public void WesternHoroscopeProducesBodiesAndHouses()
		{
			var service = new AstroScopeService();
			DateTime dateTimeUtc = new DateTime(2024, 2, 4, 3, 0, 0, DateTimeKind.Utc);
			var result = service.ComputeWestern(dateTimeUtc, 35.6895, 139.6917);

			Assert.AreEqual(10, result.Bodies.Count, "Expected Sun, Moon, and eight traditional planets including Pluto.");
			Assert.AreEqual(12, result.Houses.Houses.Count);
			Assert.That(result.Houses.AscendantDegrees, Is.GreaterThanOrEqualTo(0).And.LessThan(360));
		}

		[Test]
		public void EasternAstrologyComputesNineStarKi()
		{
			var service = new AstroScopeService();
			var tz = Jst;
			// 立春 2024 は 2/4 17:27 JST なので、年盤が 2024（三碧）に切り替わった翌日を使う。
			DateTime local = new DateTime(2024, 2, 5, 12, 0, 0, DateTimeKind.Unspecified);
			var result = service.ComputeEastern(local, 139.6917, tz);

			Assert.That(result.LunarAgeDays, Is.GreaterThan(0).And.LessThan(30));
			Assert.AreEqual(3, result.Chart.YearStar);
			Assert.That(result.Chart.MonthStar, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(9));
			Assert.That(result.Chart.DayStar, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(9));
		}

		// 旧暦 2033 年問題: 冬至を含む月を 11 月に固定する時憲暦式の置閏（冬至優先案）。
		[TestCase(2033, 8, 25, 2033, 8, 1, false)]
		[TestCase(2033, 12, 7, 2033, 11, 0, false)]
		[TestCase(2034, 1, 5, 2033, 11, 0, true)]
		[TestCase(2034, 2, 19, 2034, 1, 1, false)]
		// 通常年: 2025 閏6月、1 月に始まる 12 月は前年扱い。
		[TestCase(2025, 7, 25, 2025, 6, 1, true)]
		[TestCase(2026, 1, 19, 2025, 12, 1, false)]
		public void LunisolarDateFollowsWinterSolsticeRule(int year, int month, int day, int expectedYear, int expectedMonth, int expectedDay, bool expectedLeap)
		{
			var result = ComputeEasternTokyo(year, month, day);

			Assert.AreEqual(expectedYear, result.LunisolarDate.Year, "year");
			Assert.AreEqual(expectedMonth, result.LunisolarDate.Month, "month");
			Assert.AreEqual(expectedLeap, result.LunisolarDate.IsLeapMonth, "leap");
			if (expectedDay > 0)
			{
				Assert.AreEqual(expectedDay, result.LunisolarDate.Day, "day");
			}
		}

		// 節月: 2026 丙午年（一白）。寅月 = 庚寅・八白、卯月 = 辛卯・七赤。
		[TestCase(2026, 2, 10, HeavenlyStem.Geng, EarthlyBranch.Yin, 8)]
		[TestCase(2026, 3, 10, HeavenlyStem.Xin, EarthlyBranch.Mao, 7)]
		public void SolarMonthDrivesSexagenaryAndNineStar(int year, int month, int day, HeavenlyStem expectedStem, EarthlyBranch expectedBranch, int expectedMonthStar)
		{
			var result = ComputeEasternTokyo(year, month, day);

			Assert.AreEqual(HeavenlyStem.Bing, result.Sexagenary.Year.Stem);
			Assert.AreEqual(EarthlyBranch.Wu, result.Sexagenary.Year.Branch);
			Assert.AreEqual(expectedStem, result.Sexagenary.Month.Stem);
			Assert.AreEqual(expectedBranch, result.Sexagenary.Month.Branch);
			Assert.AreEqual(1, result.Chart.YearStar);
			Assert.AreEqual(expectedMonthStar, result.Chart.MonthStar);
		}

		private static EasternAstrologyResult ComputeEasternTokyo(int year, int month, int day)
		{
			var tz = Jst;
			DateTime local = new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Unspecified);
			return new AstroScopeService().ComputeEastern(local, 139.6917, tz);
		}
	}
}
#endif

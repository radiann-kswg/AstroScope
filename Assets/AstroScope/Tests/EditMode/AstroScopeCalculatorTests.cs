using System;
using NUnit.Framework;

namespace AstroScope.Tests
{
	public class AstroScopeCalculatorTests
	{
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
			var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
			DateTime local = new DateTime(2024, 2, 4, 12, 0, 0, DateTimeKind.Unspecified);
			var result = service.ComputeEastern(local, 139.6917, tz);

			Assert.That(result.LunarAgeDays, Is.GreaterThan(0).And.LessThan(30));
			Assert.AreEqual(3, result.Chart.YearStar);
			Assert.That(result.Chart.MonthStar, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(9));
			Assert.That(result.Chart.DayStar, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(9));
		}
	}
}

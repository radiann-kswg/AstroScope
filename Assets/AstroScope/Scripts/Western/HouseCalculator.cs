using System;
using System.Collections.Generic;

namespace AstroScope
{
	/// <summary>
	/// Generates house cusps, ascendant, and intercept metadata using an equal-house system.
	/// </summary>
	public class HouseCalculator
	{
		/// <summary>Computes equal-house cusps for the supplied UTC time and geographic location.</summary>
		public HouseCalculationResult ComputeEqualHouses(DateTime dateTimeUtc, double latitudeDegrees, double longitudeDegrees)
		{
			var result = new HouseCalculationResult();
			double jd = JulianDate.FromDateTime(dateTimeUtc);
			double t = JulianDate.ToJulianCenturies(jd);
			double epsilon = Angle.ToRadians(AstroTime.MeanObliquity(t));
			double lstDegrees = AstroTime.ApparentSiderealTime(dateTimeUtc, longitudeDegrees);
			double lst = Angle.ToRadians(lstDegrees);
			double latitude = Angle.ToRadians(latitudeDegrees);

			double sinAsc = -Math.Cos(lst);
			double cosAsc = Math.Sin(lst) * Math.Cos(epsilon) + Math.Tan(latitude) * Math.Sin(epsilon);
			double ascendant = Angle.NormalizeDegrees(Angle.ToDegrees(Math.Atan2(sinAsc, cosAsc)));

			double sinMc = Math.Sin(lst) * Math.Cos(epsilon) - Math.Tan(latitude) * Math.Sin(epsilon);
			double cosMc = Math.Cos(lst);
			double midheaven = Angle.NormalizeDegrees(Angle.ToDegrees(Math.Atan2(sinMc, cosMc)));

			var houses = new List<HousePosition>(12);
			for (int i = 0; i < 12; i++)
			{
				houses.Add(new HousePosition(i + 1, ascendant + 30.0 * i));
			}

			result.AscendantDegrees = ascendant;
			result.MidheavenDegrees = midheaven;
			result.Houses = houses;
			AnnotateHouseMetadata(result);
			return result;
		}

		private static void AnnotateHouseMetadata(HouseCalculationResult result)
		{
			var houseCusps = new Dictionary<int, ZodiacSign>(12);
			var signsOnCusps = new List<ZodiacSign>(12);
			var signPresence = new bool[12];

			foreach (var house in result.Houses)
			{
				var sign = ZodiacUtility.GetSign(house.Longitude);
				houseCusps[house.HouseNumber] = sign;
				if (!signPresence[(int)sign])
				{
					signsOnCusps.Add(sign);
					signPresence[(int)sign] = true;
				}
			}

			var missingSigns = new List<ZodiacSign>();
			for (int i = 0; i < 12; i++)
			{
				if (!signPresence[i])
				{
					missingSigns.Add((ZodiacSign)i);
				}
			}

			var intercepts = new List<HouseIntercept>();
			if (missingSigns.Count > 0)
			{
				foreach (var sign in missingSigns)
				{
					var house = FindInterceptHouse(result.Houses, sign);
					if (house != null)
					{
						intercepts.Add(new HouseIntercept(house.Value.HouseNumber, sign));
					}
				}
			}

			result.HouseCusps = houseCusps;
			result.SignsOnCusps = signsOnCusps;
			result.InterceptedSigns = missingSigns;
			result.Intercepts = intercepts;
		}

		private static HousePosition? FindInterceptHouse(IReadOnlyList<HousePosition> houses, ZodiacSign sign)
		{
			double signStart = ZodiacUtility.GetSignStartLongitude(sign);
			double signEnd = ZodiacUtility.GetSignEndLongitude(sign);

			for (int i = 0; i < houses.Count; i++)
			{
				var current = houses[i];
				var next = houses[(i + 1) % houses.Count];

				double start = current.Longitude;
				double end = next.Longitude;
				double span = Angle.NormalizeDegrees(end - start);
				if (span <= 0)
				{
					span += 360.0;
				}

				double deltaStart = NormalizeForward(signStart - start);
				double deltaEnd = NormalizeForward(signEnd - start);
				if (deltaEnd < deltaStart)
				{
					deltaEnd += 360.0;
				}

				if (deltaStart > 0.0 && deltaEnd < span)
				{
					return current;
				}
			}

			return null;
		}

		private static double NormalizeForward(double angle)
		{
			double value = Angle.NormalizeDegrees(angle);
			if (value < 0)
			{
				value += 360.0;
			}
			return value;
		}
	}
}

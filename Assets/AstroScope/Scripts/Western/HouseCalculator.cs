using System;
using System.Collections.Generic;

namespace AstroScope
{
	public class HouseCalculator
	{
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
			return result;
		}
	}
}

using System;

namespace AstroScope
{
	[Serializable]
	public class LunisolarDate
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
		public bool IsLeapMonth { get; set; }
	}

	[Serializable]
	public class NineStarKiChart
	{
		public int YearStar { get; set; }
		public int MonthStar { get; set; }
		public int DayStar { get; set; }
	}

	[Serializable]
	public class EasternAstrologyResult
	{
		public DateTime DateTimeUtc { get; set; }
		public LunisolarDate LunisolarDate { get; set; } = new();
		public double LunarAgeDays { get; set; }
		public DateTime SetsubunUtc { get; set; }
		public DateTime SetsubunLocal { get; set; }
		public NineStarKiChart Chart { get; set; } = new();
	}
}

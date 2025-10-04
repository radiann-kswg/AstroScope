using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;

namespace AstroScope
{
	/// <summary>Ten Heavenly Stems (干) used in the sexagenary cycle.</summary>
	public enum HeavenlyStem
	{
		Jia = 0,
		Yi,
		Bing,
		Ding,
		Wu,
		Ji,
		Geng,
		Xin,
		Ren,
		Gui
	}

	/// <summary>Twelve Earthly Branches (支) used in the sexagenary cycle.</summary>
	public enum EarthlyBranch
	{
		Zi = 0,
		Chou,
		Yin,
		Mao,
		Chen,
		Si,
		Wu,
		Wei,
		Shen,
		You,
		Xu,
		Hai
	}

	/// <summary>Twenty-four solar terms (節気) tracked for East Asian calendrics.</summary>
	public enum SolarTermId
	{
		StartOfSpring,
		RainWater,
		AwakeningOfInsects,
		SpringEquinox,
		PureBrightness,
		GrainRain,
		StartOfSummer,
		GrainFull,
		GrainInEar,
		SummerSolstice,
		MinorHeat,
		MajorHeat,
		StartOfAutumn,
		EndOfHeat,
		WhiteDew,
		AutumnEquinox,
		ColdDew,
		FrostDescent,
		StartOfWinter,
		MinorSnow,
		MajorSnow,
		WinterSolstice,
		MinorCold,
		MajorCold
	}

	/// <summary>Four 土用 (doyō) seasonal adjustment periods.</summary>
	public enum DoyouSeason
	{
		Spring,
		Summer,
		Autumn,
		Winter
	}

	/// <summary>Combines a stem-branch pair (干支) for year/month/day cycles.</summary>
	[Serializable]
	public class SexagenaryCycle
	{
		public HeavenlyStem Stem { get; set; }
		public EarthlyBranch Branch { get; set; }
	}

	/// <summary>Sexagenary signatures for year, month, and day.</summary>
	[Serializable]
	public class SexagenaryDate
	{
		public SexagenaryCycle Year { get; set; } = new();
		public SexagenaryCycle Month { get; set; } = new();
		public SexagenaryCycle Day { get; set; } = new();
	}

	/// <summary>Represents a lunisolar date with leap month awareness.</summary>
	[Serializable]
	public class LunisolarDate
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
		public bool IsLeapMonth { get; set; }
	}

	/// <summary>Nine Star Ki (九星気学) chart values.</summary>
	[Serializable]
	public class NineStarKiChart
	{
		public int YearStar { get; set; }
		public int MonthStar { get; set; }
		public int DayStar { get; set; }
	}

	/// <summary>Timestamp for a specific solar term crossing.</summary>
	[Serializable]
	public class SolarTermEntry
	{
		public SolarTermId Id { get; set; }
		public double TargetLongitude { get; set; }
		public DateTime DateTimeUtc { get; set; }
		public DateTime DateTimeLocal { get; set; }
	}

	/// <summary>Represents a 土用 period anchored to a seasonal solar term.</summary>
	[Serializable]
	public class DoyouPeriod
	{
		public DoyouSeason Season { get; set; }
		public DateTime StartUtc { get; set; }
		public DateTime EndUtc { get; set; }
		public DateTime StartLocal { get; set; }
		public DateTime EndLocal { get; set; }
	}

	/// <summary>Combined result for Eastern astrology calculations.</summary>
	[Serializable]
	public class EasternAstrologyResult
	{
		public DateTime DateTimeUtc { get; set; }
		public LunisolarDate LunisolarDate { get; set; } = new();
		public double LunarAgeDays { get; set; }
		public DateTime SetsubunUtc { get; set; }
		public DateTime SetsubunLocal { get; set; }
		public NineStarKiChart Chart { get; set; } = new();
		public SexagenaryDate Sexagenary { get; set; } = new();
		public List<SolarTermEntry> SolarTerms { get; set; } = new();
		public List<DoyouPeriod> DoyouPeriods { get; set; } = new();
	}
}

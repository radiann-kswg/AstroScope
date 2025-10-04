using System;

namespace AstroScope
{
	/// <summary>Supported output languages for AstroScope localization.</summary>
	public enum AstroLanguage
	{
		English,
		Japanese
	}

	/// <summary>
	/// Centralized localization helper for converting enumerations into human-friendly UI strings.
	/// </summary>
	public static class AstroLocalization
	{
		private static readonly string[] PlanetNamesEnglish =
		{
			"Sun",
			"Moon",
			"Mercury",
			"Venus",
			"Mars",
			"Jupiter",
			"Saturn",
			"Uranus",
			"Neptune",
			"Pluto",
			"Earth"
		};

		private static readonly string[] PlanetNamesJapanese =
		{
			"太陽",
			"月",
			"水星",
			"金星",
			"火星",
			"木星",
			"土星",
			"天王星",
			"海王星",
			"冥王星",
			"地球"
		};

		private static readonly string[] ZodiacNamesEnglish =
		{
			"Aries",
			"Taurus",
			"Gemini",
			"Cancer",
			"Leo",
			"Virgo",
			"Libra",
			"Scorpio",
			"Sagittarius",
			"Capricorn",
			"Aquarius",
			"Pisces"
		};

		private static readonly string[] ZodiacNamesJapanese =
		{
			"牡羊座",
			"牡牛座",
			"双子座",
			"蟹座",
			"獅子座",
			"乙女座",
			"天秤座",
			"蠍座",
			"射手座",
			"山羊座",
			"水瓶座",
			"魚座"
		};

		private static readonly string[] HeavenlyStemsEnglish =
		{
			"Jia",
			"Yi",
			"Bing",
			"Ding",
			"Wu",
			"Ji",
			"Geng",
			"Xin",
			"Ren",
			"Gui"
		};

		private static readonly string[] HeavenlyStemsJapanese =
		{
			"甲",
			"乙",
			"丙",
			"丁",
			"戊",
			"己",
			"庚",
			"辛",
			"壬",
			"癸"
		};

		private static readonly string[] EarthlyBranchesEnglish =
		{
			"Rat",
			"Ox",
			"Tiger",
			"Rabbit",
			"Dragon",
			"Snake",
			"Horse",
			"Goat",
			"Monkey",
			"Rooster",
			"Dog",
			"Pig"
		};

		private static readonly string[] EarthlyBranchesJapanese =
		{
			"子",
			"丑",
			"寅",
			"卯",
			"辰",
			"巳",
			"午",
			"未",
			"申",
			"酉",
			"戌",
			"亥"
		};

		private static readonly string[] SolarTermNamesEnglish =
		{
			"Start of Spring",
			"Rain Water",
			"Awakening of Insects",
			"Spring Equinox",
			"Pure Brightness",
			"Grain Rain",
			"Start of Summer",
			"Grain Full",
			"Grain in Ear",
			"Summer Solstice",
			"Minor Heat",
			"Major Heat",
			"Start of Autumn",
			"End of Heat",
			"White Dew",
			"Autumn Equinox",
			"Cold Dew",
			"Frost Descent",
			"Start of Winter",
			"Minor Snow",
			"Major Snow",
			"Winter Solstice",
			"Minor Cold",
			"Major Cold"
		};

		private static readonly string[] SolarTermNamesJapanese =
		{
			"立春",
			"雨水",
			"啓蟄",
			"春分",
			"清明",
			"穀雨",
			"立夏",
			"小満",
			"芒種",
			"夏至",
			"小暑",
			"大暑",
			"立秋",
			"処暑",
			"白露",
			"秋分",
			"寒露",
			"霜降",
			"立冬",
			"小雪",
			"大雪",
			"冬至",
			"小寒",
			"大寒"
		};

		private static readonly string[] NineStarNamesEnglish =
		{
			"One White Water Star",
			"Two Black Earth Star",
			"Three Azure Wood Star",
			"Four Green Wood Star",
			"Five Yellow Earth Star",
			"Six White Metal Star",
			"Seven Crimson Metal Star",
			"Eight White Earth Star",
			"Nine Purple Fire Star"
		};

		private static readonly string[] NineStarNamesJapanese =
		{
			"一白水星",
			"二黒土星",
			"三碧木星",
			"四緑木星",
			"五黄土星",
			"六白金星",
			"七赤金星",
			"八白土星",
			"九紫火星"
		};

		private static readonly string[] DoyouNamesEnglish =
		{
			"Spring Doyo",
			"Summer Doyo",
			"Autumn Doyo",
			"Winter Doyo"
		};

		private static readonly string[] DoyouNamesJapanese =
		{
			"春土用",
			"夏土用",
			"秋土用",
			"冬土用"
		};

		public static string GetPlanetName(PlanetId id, AstroLanguage language)
		{
			int index = (int)id;
			return language switch
			{
				AstroLanguage.Japanese => PlanetNamesJapanese[index],
				_ => PlanetNamesEnglish[index]
			};
		}

		public static string GetZodiacName(ZodiacSign sign, AstroLanguage language)
		{
			int index = (int)sign;
			return language switch
			{
				AstroLanguage.Japanese => ZodiacNamesJapanese[index],
				_ => ZodiacNamesEnglish[index]
			};
		}

		public static string GetHeavenlyStemName(HeavenlyStem stem, AstroLanguage language)
		{
			int index = (int)stem;
			return language switch
			{
				AstroLanguage.Japanese => HeavenlyStemsJapanese[index],
				_ => HeavenlyStemsEnglish[index]
			};
		}

		public static string GetEarthlyBranchName(EarthlyBranch branch, AstroLanguage language)
		{
			int index = (int)branch;
			return language switch
			{
				AstroLanguage.Japanese => EarthlyBranchesJapanese[index],
				_ => EarthlyBranchesEnglish[index]
			};
		}

		public static string GetSolarTermName(SolarTermId id, AstroLanguage language)
		{
			int index = (int)id;
			return language switch
			{
				AstroLanguage.Japanese => SolarTermNamesJapanese[index],
				_ => SolarTermNamesEnglish[index]
			};
		}

		public static string GetLunisolarString(EasternAstrologyResult eastern, AstroLanguage language)
		{
			return language == AstroLanguage.Japanese
					? $"{eastern.LunisolarDate.Year}年 {eastern.LunisolarDate.Month}月 {(eastern.LunisolarDate.IsLeapMonth ? "(閏) " : string.Empty)}{eastern.LunisolarDate.Day}日"
					: $"{eastern.LunisolarDate.Year}-{eastern.LunisolarDate.Month:D2}-{eastern.LunisolarDate.Day:D2}{(eastern.LunisolarDate.IsLeapMonth ? " (Leap)" : string.Empty)}";
		}

		public static string GetLunarAgeString(double lunarAge, AstroLanguage language)
		{
			if (language == AstroLanguage.Japanese)
			{
				return $"{lunarAge:F2} 夜";
			}
			else
			{
				return $"{lunarAge:F2} days";
			}
		}

		public static string GetNineStarName(int id, AstroLanguage language)
		{
			int index = id - 1;
			return language switch
			{
				AstroLanguage.Japanese => NineStarNamesJapanese[index],
				_ => NineStarNamesEnglish[index]
			};
		}

		public static string GetDoyouName(DoyouSeason season, AstroLanguage language)
		{
			int index = (int)season;
			return language switch
			{
				AstroLanguage.Japanese => DoyouNamesJapanese[index],
				_ => DoyouNamesEnglish[index]
			};
		}

		public static string GetSexagenaryName(SexagenaryCycle cycle, AstroLanguage language)
		{
			return GetHeavenlyStemName(cycle.Stem, language) + GetEarthlyBranchName(cycle.Branch, language);
		}

		public static string GetHouseLabel(int houseNumber, AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => $"第{houseNumber}室",
				_ => $"House {houseNumber}"
			};
		}

		public static string GetAscendantLabel(AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => "アセンダント",
				_ => "Ascendant"
			};
		}

		public static string GetMidheavenLabel(AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => "MC",
				_ => "Midheaven"
			};
		}

		public static string GetInterceptLabel(AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => "インターセプト",
				_ => "Intercept"
			};
		}

		public static string GetMissingSignsLabel(AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => "欠落サイン",
				_ => "Missing Signs"
			};
		}

		public static string GetNoneLabel(AstroLanguage language)
		{
			return language switch
			{
				AstroLanguage.Japanese => "なし",
				_ => "None"
			};
		}
	}
}

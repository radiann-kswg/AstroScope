using System.Collections.Generic;

namespace AstroScope
{
	/// <summary>
	/// Static lookup table providing mean orbital elements and secular rates for the major planets.
	/// </summary>
	public static class OrbitalElementsDatabase
	{
		private static readonly Dictionary<PlanetId, OrbitalElements> Elements = new()
		{
			{
				PlanetId.Mercury,
				new OrbitalElements(
					0.387098310, 0.20563175, 7.004986, 48.330893, 77.456119, 252.250906,
					0.0, 0.000020407, -0.00594749, -0.125422, 0.1588643, 149474.0722491)
			},
			{
				PlanetId.Venus,
				new OrbitalElements(
					0.723329820, 0.00677192, 3.394662, 76.679920, 131.563707, 181.979801,
					-0.0000000006, -0.000047765, -0.0008568, -0.2780134, 0.0048646, 58519.2130302)
			},
			{
				PlanetId.Earth,
				new OrbitalElements(
					1.000001018, 0.01670863, 0.00005, -11.26064, 102.937348, 100.466457,
					0.0000000002, -0.000042037, -0.0000001, -0.24123856, 0.3225557, 35999.3728565)
			},
			{
				PlanetId.Mars,
				new OrbitalElements(
					1.523679342, 0.09340065, 1.849726, 49.558093, 336.060234, 355.433000,
					0.00000019026, 0.000090484, -0.0081479, -0.29257343, 0.4438898, 19140.2993039)
			},
			{
				PlanetId.Jupiter,
				new OrbitalElements(
					5.202603191, 0.04848264, 1.303270, 100.464407, 14.331207, 34.351519,
					0.0000001913, -0.000163244, -0.0019872, 0.1767232, 0.2155209, 3034.9056606)
			},
			{
				PlanetId.Saturn,
				new OrbitalElements(
					9.554909596, 0.05550862, 2.488878, 113.665503, 93.057237, 50.077444,
					-0.0000021389, -0.000346818, 0.0025514, 0.877088, 0.5665415, 1222.1138488)
			},
			{
				PlanetId.Uranus,
				new OrbitalElements(
					19.218446061, 0.04629590, 0.773196, 74.005947, 173.005159, 314.055005,
					-0.0000000372, -0.000027337, -0.0016869, 0.5211258, 0.0893206, 428.4669983)
			},
			{
				PlanetId.Neptune,
				new OrbitalElements(
					30.110386869, 0.00898809, 1.769952, 131.784057, 48.123691, 304.348665,
					-0.0000001663, 0.000006408, 0.0002557, -0.0061651, 0.0291587, 218.4862002)
			},
			{
				PlanetId.Pluto,
				new OrbitalElements(
					39.48211675, 0.24882730, 17.140012, 110.303936, 224.068916, 238.929038,
					-0.00031596, 0.00005170, -0.00004818, -0.01183482, -0.04062942, 145.20780515)
			}
		};

		public static OrbitalElements GetElements(PlanetId planet)
		{
			return Elements[planet];
		}
	}
}

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WorldForge.Builders.PostProcessors
{
	public class CommonWeightmapColors
	{
		public static Color white = Color.FromRgb(255, 255, 255);
		public static Color black = Color.FromRgb(0, 0, 0);

		//Primary colors
		public static Color red = Color.FromRgb(255, 0, 0);
		public static Color green = Color.FromRgb(0, 255, 0);
		public static Color blue = Color.FromRgb(0, 0, 255);

		//Secondary colors
		public static Color yellow = Color.FromRgb(255, 255, 0);
		public static Color cyan = Color.FromRgb(0, 255, 255);
		public static Color magenta = Color.FromRgb(255, 0, 255);

		public static Color NameToColor(string s)
		{
			switch(s.ToLower())
			{
				case "w":
				case "white": return white;
				case "k":
				case "black": return black;
				case "r":
				case "red": return red;
				case "g":
				case "green": return green;
				case "b":
				case "blue": return blue;
				case "y":
				case "yellow": return yellow;
				case "c":
				case "cyan": return cyan;
				case "m":
				case "magenta": return magenta;
				default: Console.WriteLine("Unknown common color: " + s); return black;
			}
		}
	} 
}
using ImageMagick;
using System;

namespace WorldForge.Builders.PostProcessors
{
	public class CommonWeightmapColors
	{
		public static IMagickColor<byte> white = MagickColor.FromRgb(255, 255, 255);
		public static IMagickColor<byte> black = MagickColor.FromRgb(0, 0, 0);

		//Primary colors
		public static IMagickColor<byte> red = MagickColor.FromRgb(255, 0, 0);
		public static IMagickColor<byte> green = MagickColor.FromRgb(0, 255, 0);
		public static IMagickColor<byte> blue = MagickColor.FromRgb(0, 0, 255);

		//Secondary colors
		public static IMagickColor<byte> yellow = MagickColor.FromRgb(255, 255, 0);
		public static IMagickColor<byte> cyan = MagickColor.FromRgb(0, 255, 255);
		public static IMagickColor<byte> magenta = MagickColor.FromRgb(255, 0, 255);

		public static IMagickColor<byte> NameToColor(string s)
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
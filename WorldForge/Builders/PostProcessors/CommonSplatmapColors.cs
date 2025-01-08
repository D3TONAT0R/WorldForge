using System;

namespace WorldForge.Builders.PostProcessors
{
	public class CommonWeightmapColors
	{
		public static BitmapColor white = new BitmapColor(255, 255, 255);
		public static BitmapColor black = new BitmapColor(0, 0, 0);

		//Primary colors
		public static BitmapColor red = new BitmapColor(255, 0, 0);
		public static BitmapColor green = new BitmapColor(0, 255, 0);
		public static BitmapColor blue = new BitmapColor(0, 0, 255);

		//Secondary colors
		public static BitmapColor yellow = new BitmapColor(255, 255, 0);
		public static BitmapColor cyan = new BitmapColor(0, 255, 255);
		public static BitmapColor magenta = new BitmapColor(255, 0, 255);

		public static BitmapColor NameToColor(string s)
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
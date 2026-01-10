using System;

namespace WorldForge
{
	public enum ColorType : byte
	{
		White = 0,
		Orange = 1,
		Magenta = 2,
		LightBlue = 3,
		Yellow = 4,
		Lime = 5,
		Pink = 6,
		Gray = 7,
		LightGray = 8,
		Cyan = 9,
		Purple = 10,
		Blue = 11,
		Brown = 12,
		Green = 13,
		Red = 14,
		Black = 15
	}

	public enum ColorTypeInt : int
	{
		White = 0,
		Orange = 1,
		Magenta = 2,
		LightBlue = 3,
		Yellow = 4,
		Lime = 5,
		Pink = 6,
		Gray = 7,
		Silver = 8,
		Cyan = 9,
		Purple = 10,
		Blue = 11,
		Brown = 12,
		Green = 13,
		Red = 14,
		Black = 15
	}

	public static class ColorTypeExtensions
	{
		public static string ToLowercaseString(this ColorType c)
		{
			switch(c)
			{
				case ColorType.White: return "white";
				case ColorType.Orange: return "orange";
				case ColorType.Magenta: return "magenta";
				case ColorType.LightBlue: return "light_blue";
				case ColorType.Yellow: return "yellow";
				case ColorType.Lime: return "lime";
				case ColorType.Pink: return "pink";
				case ColorType.Gray: return "gray";
				case ColorType.LightGray: return "light_gray";
				case ColorType.Cyan: return "cyan";
				case ColorType.Purple: return "purple";
				case ColorType.Blue: return "blue";
				case ColorType.Brown: return "brown";
				case ColorType.Green: return "green";
				case ColorType.Red: return "red";
				case ColorType.Black: return "black";
				default: throw new ArgumentException();
			}
		}

		public static ColorType ParseColorType(this string s)
		{
			switch(s.ToLower())
			{
				case "white": return ColorType.White;
				case "orange": return ColorType.Orange;
				case "magenta": return ColorType.Magenta;
				case "light_blue": return ColorType.LightBlue;
				case "yellow": return ColorType.Yellow;
				case "lime": return ColorType.Lime;
				case "pink": return ColorType.Pink;
				case "gray": return ColorType.Gray;
				case "light_gray": return ColorType.LightGray;
				case "cyan": return ColorType.Cyan;
				case "purple": return ColorType.Purple;
				case "blue": return ColorType.Blue;
				case "brown": return ColorType.Brown;
				case "green": return ColorType.Green;
				case "red": return ColorType.Red;
				case "black": return ColorType.Black;
				default: throw new ArgumentException("Unknown color type: " + s);
			}
		}
	}
}

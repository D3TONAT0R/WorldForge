using SixLabors.ImageSharp.PixelFormats;

namespace WorldForge
{
	public static class ColorExtensions
	{
		public static byte GetChannel(this Rgba32 color, ColorChannel channel)
		{
			switch(channel)
			{
				case ColorChannel.Red: return color.R;
				case ColorChannel.Green: return color.G;
				case ColorChannel.Blue: return color.B;
				case ColorChannel.Alpha: return color.A;
				default: return 0;
			}
		}
	}
}
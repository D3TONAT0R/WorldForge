using ImageMagick;

namespace WorldForge
{
	public static class ImageMagickExtensions
	{

		public static IMagickColor<byte> GetPixelColor(this IPixelCollection<byte> pixels, int x, int y)
		{
			var pixel = pixels.GetPixel(x, y);
			var color = pixel.ToColor();
			return color;
		}

		public static byte GetChannel(this IMagickColor<byte> color, ColorChannel channel)
		{
			switch(channel)
			{
				case ColorChannel.Red:
					return color.R;
				case ColorChannel.Green:
					return color.G;
				case ColorChannel.Blue:
					return color.B;
				case ColorChannel.Alpha:
					return color.A;
				default:
					return 0;
			}
		}
	}
}

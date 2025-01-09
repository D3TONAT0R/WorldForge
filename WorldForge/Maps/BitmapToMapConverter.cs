using System;
using System.Linq;

namespace WorldForge.Maps
{
	//TODO: converted images look very bad compared to online tools such as https://mc-map.djfun.de/
	public static class BitmapToMapConverter
	{
		public static MapData GenerateMap(IBitmap bitmap, Boundary? boundary = null, MapColorPalette palette = null, int dithering = 0)
		{
			palette = palette ?? MapColorPalette.Modern;
			if(boundary == null)
			{
				if(bitmap.Width != 128 || bitmap.Height != 128)
				{
					throw new ArgumentException("GenerateMap must be called with an input bitmap that is 128x128 or with a 128x128 boundary");
				}
			}
			else
			{
				var b = boundary.Value;
				if(b.LengthX != 128 || b.LengthZ != 128)
				{
					throw new ArgumentException("Bitmap boundary must be 128x128");
				}
				bitmap = bitmap.CloneArea(b.xMin, b.zMin, b.LengthX, b.LengthZ);
			}
			var map = new MapData(DimensionID.Unknown, 0, 0)
			{
				locked = true,
				trackingPosition = false
			};
			var paletteColors = palette.mapColorPalette.SelectMany(ct => ct.GetColors()).Select(Gamma).ToArray();
			for(int z = 0; z < 128; z++)
			{
				for(int x = 0; x < 128; x++)
				{
					var color = Gamma(bitmap.GetPixel(x, z));
					byte index;
					if(dithering > 0) index = (byte)BitmapColor.GetDitheredMapping(x, z, color, paletteColors, dithering);
					else index = (byte)BitmapColor.GetClosestMapping(color, paletteColors);
					map.SetColorIndex(x, z, index);
				}
			}
			return map;
		}

		private static BitmapColor Gamma(BitmapColor color) => new BitmapColor(
			(byte)(Math.Pow(color.r / 255.0, 0.4545) * 255),
			(byte)(Math.Pow(color.g / 255.0, 0.4545) * 255),
			(byte)(Math.Pow(color.b / 255.0, 0.4545) * 255)
		);

		private static float[] RGBToLAB(BitmapColor color)
		{
			float num = 0;

			float[] rgb = new float[3];
			for(int i = 0; i < 3; i++)
			{
				float v = color[i] / 255.0f;
				if(v > 0.04045f) v = (float)Math.Pow((v + 0.055f) / 1.055f, 2.4);
				else v /= 12.92f;
				rgb[i] = v;
			}

			float[] xyz = new float[3];

			xyz[0] = rgb[0] * 0.4124f + rgb[1] * 0.3576f + rgb[2] * 0.1805f;
			xyz[1] = rgb[0] * 0.2126f + rgb[1] * 0.7152f + rgb[2] * 0.0722f;
			xyz[2] = rgb[0] * 0.0193f + rgb[1] * 0.1192f + rgb[2] * 0.9505f;

			// Observer= 2°, Illuminant= D65
			xyz[0] /= 95.047f;        // ref_X =  95.047
			xyz[1] /= 100.0f;         // ref_Y = 100.000
			xyz[2] /= 108.883f;       // ref_Z = 108.883


			for(int i = 0; i < 3; i++)
			{
				if(xyz[i] > 0.008856f) xyz[i] = (float)Math.Pow(xyz[i], 0.3333333333333333);
				else xyz[i] = 7.787f * xyz[i] + 16f / 116;
			}

			float l = 116 * xyz[1] - 16;
			float a = 500 * (xyz[0] - xyz[1]);
			float b = 200 * (xyz[1] - xyz[2]);

			return new float[] { l, a, b };
		}
	}
}
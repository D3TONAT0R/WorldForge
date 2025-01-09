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
			var paletteColors = palette.mapColorPalette.SelectMany(ct => ct.GetColors()).ToArray();
			for(int z = 0; z < 128; z++)
			{
				for(int x = 0; x < 128; x++)
				{
					var color = bitmap.GetPixel(x, z);
					byte index;
					if(dithering > 0) index = (byte)BitmapColor.GetDitheredMapping(x, z, color, paletteColors, dithering);
					else index = (byte)BitmapColor.GetClosestMapping(color, paletteColors);
					map.SetColorIndex(x, z, index);
				}
			}
			return map;
		}
	}
}
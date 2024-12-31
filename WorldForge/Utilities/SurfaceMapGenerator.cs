using ImageMagick;
using System;
using System.Linq;
using WorldForge.Coordinates;

namespace WorldForge
{
	public static class SurfaceMapGenerator
	{
		public static MagickImage GenerateHeightMap(Dimension dim, int xMin, int zMin, int xMax, int zMax, HeightmapType surfaceType)
		{
			var heightmap = dim.GetHeightmap(xMin, zMin, xMax, zMax, surfaceType, true);
			var image = new MagickImage(MagickColor.FromRgb(0, 0, 0), (uint)heightmap.GetLength(0), (uint)heightmap.GetLength(1));
			var pixels = image.GetPixels();
			var color = new byte[image.ChannelCount];
			for(int z = zMin; z < zMax; z++)
			{
				for(int x = xMin; x < xMax; x++)
				{
					int y = heightmap[x - xMin, z - zMin];
					if(y < 0) continue;
					byte brt = (byte)Math.Max(Math.Min(y, 255), 0);
					for(int i = 0; i < color.Length; i++)
					{
						color[i] = brt;
					}
					pixels.SetPixel(x - xMin, z - zMin, color);
				}
			}
			return image;
		}

		/// <summary>
		/// Generates a colored overview map from the specified area (With Z starting from top)
		/// </summary>
		public static MagickImage GenerateSurfaceMap(Dimension dim, int xMin, int zMin, int xMax, int zMax, HeightmapType surfaceType, bool shading)
		{
			//TODO: beta regions are not loaded
			var heightmap = dim.GetHeightmap(xMin, zMin, xMax, zMax, surfaceType);
			var image = new MagickImage(MagickColors.Transparent, (uint)heightmap.GetLength(0), (uint)heightmap.GetLength(1));
			var pixels = image.GetPixels();
			for(int z = zMin; z <= zMax; z++)
			{
				for(int x = xMin; x <= xMax; x++)
				{
					int y = heightmap[x - xMin, z - zMin];
					if(y < 0) continue;
					var block = dim.GetBlock(new BlockCoord(x, y, z));
					int shade = 0;
					if(block != null && shading)
					{
						shade = GetShade(dim, xMin, zMin, z, block, x, y, heightmap);
					}

					//Check for snow above the block
					var aboveBlock = dim.GetBlock((x, y + 1, z));
					if(aboveBlock != null && aboveBlock.ID.Matches("minecraft:snow")) block = aboveBlock;

					pixels.SetPixel(x - xMin, z - zMin, Blocks.GetMapColor(block, shade).ToByteArray());
				}
			}
			return image;
		}

		private static int GetShade(Dimension dim, int xMin, int zMin, int z, BlockID block, int x, int y, short[,] heightmap)
		{
			int shade = 0;
			if(z - 1 >= zMin)
			{
				if(block != null && block.IsLiquid)
				{
					//Water dithering
					var depth = dim.GetWaterDepth(new BlockCoord(x, y, z));
					if(depth < 8) shade = 1;
					else if(depth < 16) shade = 0;
					else shade = -1;
					if(depth % 8 >= 4 && shade > -1)
					{
						if(x.Mod(2) == z.Mod(2)) shade--;
					}
				}
				else
				{
					var above = heightmap[x - xMin, z - 1 - zMin];
					if(above > y) shade = -1;
					else if(above < y) shade = 1;
				}
			}

			return shade;
		}
	}
}

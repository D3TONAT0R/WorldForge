using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using WorldForge.Coordinates;

namespace WorldForge
{
	public static class SurfaceMapGenerator
	{
		public static Image<Rgba32> GenerateHeightMap(Dimension dim, Boundary boundary, HeightmapType surfaceType)
		{
			var heightmap = dim.GetHeightmap(boundary, surfaceType, true);
			var image = new Image<Rgba32>(heightmap.GetLength(0), heightmap.GetLength(1));
			for(int z = boundary.zMin; z < boundary.zMax; z++)
			{
				for(int x = boundary.xMin; x < boundary.xMax; x++)
				{
					int y = heightmap[x - boundary.xMin, z - boundary.zMin];
					if(y < 0) continue;
					byte brt = (byte)Math.Max(Math.Min(y, 255), 0);
					image[x - boundary.xMin, z - boundary.zMin] = new Rgba32(brt, brt, brt);
				}
			}
			return image;
		}

		/// <summary>
		/// Generates a colored overview map from the specified area (With Z starting from top)
		/// </summary>
		public static Image<Rgba32> GenerateSurfaceMap(Dimension dim, Boundary boundary, HeightmapType surfaceType, bool shading)
		{
			//TODO: beta regions are not loaded
			var heightmap = dim.GetHeightmap(boundary, surfaceType);
			var image = new Image<Rgba32>(heightmap.GetLength(0), heightmap.GetLength(1), new Rgba32(0, 0, 0, 0));
			for(int z = boundary.zMin; z < boundary.zMax; z++)
			{
				for(int x = boundary.xMin; x < boundary.xMax; x++)
				{
					int y = heightmap[x - boundary.xMin, z - boundary.zMin];
					if(y < 0) continue;
					var block = dim.GetBlock(new BlockCoord(x, y, z));
					int shade = 0;
					if(block != null && shading)
					{
						shade = GetShade(dim, boundary.xMin, boundary.zMin, z, block, x, y, heightmap);
					}

					//Check for snow above the block
					var aboveBlock = dim.GetBlock((x, y + 1, z));
					if(aboveBlock != null && aboveBlock.ID.Matches("minecraft:snow")) block = aboveBlock;

					image[x - boundary.xMin, z - boundary.zMin] = BlockMapColors.GetColor(block, shade);
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
						//Same as modulo of 2
						if((x & 1) == (z & 1)) shade--;
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

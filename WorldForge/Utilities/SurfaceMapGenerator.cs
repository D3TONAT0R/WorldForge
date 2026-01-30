using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.Maps;
using WorldForge.Regions;

namespace WorldForge
{
	public static class SurfaceMapGenerator
	{
		public static IBitmap GenerateHeightMap(Dimension dim, Boundary boundary, HeightmapType surfaceType, bool forceManualHeightmapCalculation = false)
		{
			var heightmap = dim.GetHeightmap(boundary, surfaceType, forceManualHeightmapCalculation);
			if(Bitmaps.BitmapFactory == null)
			{
				throw new ArgumentNullException("No bitmap factory was provided.");
			}
			var bmp = Bitmaps.BitmapFactory.Create(heightmap.GetLength(0), heightmap.GetLength(1));
			for(int z = boundary.zMin; z < boundary.zMax; z++)
			{
				for(int x = boundary.xMin; x < boundary.xMax; x++)
				{
					int y = heightmap[x - boundary.xMin, z - boundary.zMin];
					if(y < 0) continue;
					byte brt = (byte)Math.Max(Math.Min(y, 255), 0);
					bmp.SetPixel(x - boundary.xMin, z - boundary.zMin, new BitmapColor(brt, brt, brt));
				}
			}
			return bmp;
		}

		/// <summary>
		/// Generates a colored overview map from the specified area (With Z starting from top)
		/// </summary>
		public static IBitmap GenerateSurfaceMap(Dimension dim, Boundary boundary, HeightmapType surfaceType, bool shading, MapColorPalette colorPalette, bool forceManualHeightmapCalculation = false)
		{
			//TODO: beta regions are not loaded
			if(Bitmaps.BitmapFactory == null)
			{
				throw new ArgumentNullException("No bitmap factory was provided.");
			}
			var heightmap = dim.GetHeightmap(boundary, surfaceType, forceManualHeightmapCalculation);
			var bmp = Bitmaps.BitmapFactory.Create(heightmap.GetLength(0), heightmap.GetLength(1));
			for(int z = boundary.zMin; z < boundary.zMax; z++)
			{
				for(int x = boundary.xMin; x < boundary.xMax; x++)
				{
					int y = heightmap[x - boundary.xMin, z - boundary.zMin];
					if(y == short.MinValue) continue;
					var block = dim.GetBlock(new BlockCoord(x, y, z));
					int shade = 0;
					if(block != null && shading)
					{
						shade = GetShade(dim, boundary.xMin, boundary.zMin, z, block, x, y, heightmap);
					}

					//Check for snow above the block
					var aboveBlock = dim.GetBlock((x, y + 1, z));
					if(aboveBlock != null && aboveBlock.ID.Matches("minecraft:snow")) block = aboveBlock;

					bmp.SetPixel(x - boundary.xMin, z - boundary.zMin, MapColorPalette.Modern.GetColor(block, shade));
				}
			}
			return bmp;
		}

		public static IBitmap GenerateSurfaceMap(Dimension dim, Boundary boundary, HeightmapType surfaceType, bool shading, bool forceManualHeightmapCalculation = false)
		{
			return GenerateSurfaceMap(dim, boundary, surfaceType, shading, MapColorPalette.Modern, forceManualHeightmapCalculation);
		}

		public static IBitmap GenerateSurfaceMap(Region r, HeightmapType surfaceType, bool shading, MapColorPalette colorPalette, bool forceManualHeightmapCalculation = false)
		{
			var dim = Dimension.CreateNew(null, DimensionID.Unknown, BiomeID.TheVoid);
			dim.AddRegion(r, true);
			var pos = r.regionPos;
			var boundary = new Boundary(pos.x * 512, pos.z * 512, pos.x * 512 + 512, pos.z * 512 + 512);
			return GenerateSurfaceMap(dim, boundary, surfaceType, shading, forceManualHeightmapCalculation);
		}

		public static IBitmap GenerateSurfaceMap(Chunk c, HeightmapType surfaceType, bool shading, MapColorPalette colorPalette, bool forceManualHeightmapCalculation = false)
		{
			if(Bitmaps.BitmapFactory == null)
			{
				throw new ArgumentNullException("No bitmap factory was provided.");
			}
			var heightmap = c.GetHeightmap(surfaceType, forceManualHeightmapCalculation);
			var bmp = Bitmaps.BitmapFactory.Create(16, 16);
			for(int z = 0; z < 16; z++)
			{
				for(int x = 0; x < 16; x++)
				{
					int y = heightmap[x, z];
					if(y < 0) continue;
					var block = c.GetBlock(new BlockCoord(x, y, z))?.Block;
					int shade = 0;
					if(block != null && shading)
					{
						shade = GetShade(c, block, x, y, z, heightmap);
					}

					//Check for snow above the block
					var aboveBlock = c.GetBlock((x, y + 1, z))?.Block;
					if(aboveBlock != null && aboveBlock.ID.Matches("minecraft:snow")) block = aboveBlock;

					bmp.SetPixel(x, z, MapColorPalette.Modern.GetColor(block, shade));
				}
			}
			return bmp;
		}

		public static BitmapColor GetSurfaceMapColor(Chunk c, int x, int z, HeightmapType surfaceType, MapColorPalette colorPalette)
		{
			var y = c.GetHighestBlock(x, z, surfaceType);
			if(y < 0) return new BitmapColor(0, 0, 0, 0);
			var block = c.GetBlock(new BlockCoord(x, y, z))?.Block;
			//Check for snow above the block
			var aboveBlock = c.GetBlock((x, y + 1, z))?.Block;
			if(aboveBlock != null && aboveBlock.ID.Matches("minecraft:snow")) block = aboveBlock;
			return MapColorPalette.Modern.GetColor(block, 0);
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

		private static int GetShade(Chunk c, BlockID block, int x, int y, int z, short[,] heightmap)
		{
			int shade = 0;
			if(block != null && block.IsLiquid)
			{
				var pos = new BlockCoord(x, y, z);
				//Water dithering
				int depth = 0;
				var b1 = c.GetBlock(pos).Block;
				while(b1.IsWater)
				{
					depth++;
					pos.y--;
					var b2 = c.GetBlock(pos);
					if(b2 == null) break;
					b1 = b2.Block;
				}

				if(depth < 8) shade = 1;
				else if(depth < 16) shade = 0;
				else shade = -1;
				if(depth % 8 >= 4 && shade > -1)
				{
					//Same as modulo of 2
					if((x & 1) == (z & 1)) shade--;
				}
			}
			else if(z - 1 >= 0 && heightmap != null)
			{
				var above = heightmap[x, z - 1];
				if(above > y) shade = -1;
				else if(above < y) shade = 1;
			}
			return shade;
		}
	}
}

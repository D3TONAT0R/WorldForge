using System;
using System.Collections.Generic;
using System.IO;

namespace WorldForge
{
	public static class Blocks
	{

		public static readonly string[] commonTerrainBlocks = new string[]
		{
			"minecraft:bedrock",
			"minecraft:stone",
			"minecraft:grass_block",
			"minecraft:dirt",
			"minecraft:sand",
			"minecraft:gravel",
			"minecraft:coarse_dirt",
			"minecraft:sandstone",
			"minecraft:granite",
			"minecraft:andesite",
			"minecraft:diorite",
			"minecraft:grass_path",
			"minecraft:clay"
		};

		public static readonly string[] terrainSurfaceBlocks = new string[]
		{
			"minecraft:grass_block",
			"minecraft:dirt",
			"minecraft:sand",
			"minecraft:coarse_dirt",
			"minecraft:sandstone",
			"minecraft:gravel"
		};

		public static readonly string[] plantSustainingBlocks = new string[]
		{
			"minecraft:grass_block",
			"minecraft:dirt",
			"minecraft:podzol"
		};

		public static readonly string waterBlock = "minecraft:water";
		public static readonly string lavaBlock = "minecraft:lava";

		public static readonly Dictionary<NamespacedID, int> colorMapIndices = new Dictionary<NamespacedID, int>
		{
			{new NamespacedID("minecraft:grass_block"), 0 },
			{new NamespacedID("minecraft:dirt"), 1 },
			{new NamespacedID("minecraft:coarse_dirt"), 1 },
			{new NamespacedID("minecraft:podzol"), 1 },
			{new NamespacedID("minecraft:water"), 2 },
			{new NamespacedID("minecraft:oak_leaves"),3 },
			{new NamespacedID("minecraft:birch_leaves"), 3 },
			{new NamespacedID("minecraft:spruce_leaves"), 3 },
			{new NamespacedID("minecraft:jungle_leaves"),3 },
			{new NamespacedID("minecraft:acacia_leaves"), 3 },
			{new NamespacedID("minecraft:dark_oak_leaves"),3 },
			{new NamespacedID("minecraft:azalea_leaves"),3 },
			{new NamespacedID("minecraft:stone"),4 },
			{new NamespacedID("minecraft:diorite"),4 },
			{new NamespacedID("minecraft:granite"),4 },
			{new NamespacedID("minecraft:andesite"),4 },
			{new NamespacedID("minecraft:gravel"),4 },
			{new NamespacedID("minecraft:oak_log"),5 },
			{new NamespacedID("minecraft:birch_log"),5 },
			{new NamespacedID("minecraft:spruce_log"),5 },
			{new NamespacedID("minecraft:jungle_log"),5 },
			{new NamespacedID("minecraft:acacia_log"),5 },
			{new NamespacedID("minecraft:dark_oak_log"),5 },
			{new NamespacedID("minecraft:snow"),6 },
			{new NamespacedID("minecraft:snow_block"),6 },
			{new NamespacedID("minecraft:sand"),7 },
			{new NamespacedID("minecraft:sandstone"),7 },
			{new NamespacedID("minecraft:cobblestone"),8 },
			{new NamespacedID("minecraft:bedrock"),8 },
			{new NamespacedID("minecraft:obsidian"), 8 },
			{new NamespacedID("minecraft:lava"),9 },
			{new NamespacedID("minecraft:magma_block"),9 },
			{new NamespacedID("minecraft:ice"),10 },
			{new NamespacedID("minecraft:packed_ice"),10 },
			{new NamespacedID("minecraft:blue_ice"),10 },
			{new NamespacedID("minecraft:frosted_ice"),10 }
		};

		public static readonly string[] commonColors = new string[]
		{
			"white",
			"light_gray",
			"gray",
			"black",
			"red",
			"pink",
			"orange",
			"yellow",
			"brown",
			"lime",
			"green",
			"cyan",
			"light_blue",
			"blue",
			"magenta",
			"purple"
		};

		internal static BitmapColor[,] colormap;

		public static string GetRandomColor(Random r)
		{
			return commonColors[r.Next(commonColors.Length)];
		}

		public static bool IsBlockForMap(BlockID b, HeightmapType type)
		{
			if(b == null || IsAir(b)) return false;
			if(type == HeightmapType.AllBlocks)
			{
				return true;
			}
			else if(type == HeightmapType.SolidBlocks)
			{
				return !IsTransparentBlock(b);
			}
			else if(type == HeightmapType.SolidBlocksNoLiquid)
			{
				return !IsTransparentBlock(b) && !IsLiquid(b) && !b.Compare("minecraft:ice");
			}
			else if(type == HeightmapType.TerrainBlocks)
			{
				return b.CompareMultiple(commonTerrainBlocks) || b.CompareMultiple(waterBlock, lavaBlock);
			}
			else if(type == HeightmapType.TerrainBlocksNoLiquid)
			{
				return b.CompareMultiple(commonTerrainBlocks);
			}
			else
			{
				return false;
			}
		}

		public static BitmapColor GetMapColor(BlockID block, int shade)
		{
			if(block != null)
			{
				if(!colorMapIndices.TryGetValue(block.ID, out int index))
				{
					index = 15;
				}
				shade = 1 - shade;
				return colormap[index, shade];
			}
			else
			{
				return new BitmapColor(0, 0, 0, 0);
			}
		}

		public static void InitializeColorMap(Stream colorBitmapStream)
		{
			var bmp = Bitmaps.LoadFromStream(colorBitmapStream);
			colormap = new BitmapColor[bmp.Width, 3];
			for(int x = 0; x < bmp.Width; x++)
			{
				for(int y = 0; y < 3; y++)
				{
					colormap[x, y] = bmp.GetPixel(x, y);
				}
			}
		}

		public static bool IsAir(BlockID b)
		{
			if(b == null) return false;

			return b.CompareMultiple("minecraft:air", "minecraft:cave_air");
		}

		public static bool IsLiquid(BlockID b)
		{
			if(b == null) return false;
			return b.CompareMultiple(waterBlock, lavaBlock);
		}

		public static bool IsPlantSustaining(BlockID b)
		{
			if(b == null) return false;
			return b.CompareMultiple(plantSustainingBlocks);
		}

		public static bool IsTransparentBlock(BlockID b)
		{
			string shortID = b.ID.id;
			if(shortID.Contains("glass")) return true;
			if(shortID.Contains("bars")) return true;
			if(shortID.Contains("sapling")) return true;
			if(shortID.Contains("rail")) return true;
			if(shortID.Contains("tulip")) return true;
			if(shortID.Contains("mushroom")) return true;
			if(shortID.Contains("pressure_plate")) return true;
			if(shortID.Contains("button")) return true;
			if(shortID.Contains("torch")) return true;
			if(shortID.Contains("fence")) return true;
			if(shortID.Contains("door")) return true;
			if(shortID.Contains("carpet")) return true;
			switch(b.ID.FullID)
			{
				case "minecraft:air":
				case "minecraft:cave_air":
				case "minecraft:cobweb":
				case "minecraft:grass":
				case "minecraft:fern":
				case "minecraft:dead_bush":
				case "minecraft:seagrass":
				case "minecraft:sea_pickle":
				case "minecraft:dandelion":
				case "minecraft:poppy":
				case "minecraft:blue_orchid":
				case "minecraft:allium":
				case "minecraft:azure_bluet":
				case "minecraft:end_rod":
				case "minecraft:ladder":
				case "minecraft:lever":
				case "minecraft:snow":
				case "minecraft:lily_pad":
				case "minecraft:tripwire_hook":
				case "minecraft:barrier":
				case "minecraft:tall_grass":
				case "minecraft:large_fern":
				case "minecraft:sunflower":
				case "minecraft:lilac":
				case "minecraft:rose_bush":
				case "minecraft:peony":
				case "minecraft:structure_void":
				case "minecraft:turtle_egg":
				case "minecraft:redstone":
				case "minecraft:sweet_berry_bush": return true;
			}
			return false;
		}
	}
}

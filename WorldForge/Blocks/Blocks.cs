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

		public static readonly BlockID air = BlockList.Find("air");
		public static readonly BlockID caveAir = BlockList.Find("cave_air");

		public static readonly BlockID stone = BlockList.Find("stone");
		public static readonly BlockID bedrock = BlockList.Find("bedrock");
		public static readonly BlockID grassBlock = BlockList.Find("grass_block");
		public static readonly BlockID dirt = BlockList.Find("dirt");
		public static readonly BlockID sand = BlockList.Find("sand");
		public static readonly BlockID gravel = BlockList.Find("gravel");

		public static readonly BlockID water = BlockList.Find("water");
		public static readonly BlockID lava = BlockList.Find("lava");

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
				return b.CompareMultiple(commonTerrainBlocks) || b.IsLiquid;
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

		public static bool IsAir(BlockID b)
		{
			if(b == null) return false;
			return b == air || b == caveAir;
		}

		public static bool IsLiquid(BlockID b)
		{
			if(b == null) return false;
			return b == water || b == lava;
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
				case "minecraft:short_grass":
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

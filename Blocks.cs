using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using static MCUtils.ChunkData;

namespace MCUtils {
	public static class Blocks {

		public static readonly string[] commonTerrainBlocks = new string[] {
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

		public static readonly Dictionary<string, int> colorMapIndices = new Dictionary<string, int> {
			{"minecraft:grass_block", 0 },
			{"minecraft:dirt", 1 },
			{"minecraft:coarse_dirt", 1 },
			{"minecraft:podzol", 1 },
			{"minecraft:water", 2 },
			{"minecraft:ice", 2 },
			{"minecraft:oak_leaves",3 },
			{"minecraft:birch_leaves", 3 },
			{"minecraft:spruce_leaves", 3 },
			{"minecraft:jungle_leaves",3 },
			{"minecraft:acacia_leaves", 3 },
			{"minecraft:dark_oak_leaves",3 },
			{"minecraft:azalea_leaves",3 },
			{"minecraft:stone",4 },
			{"minecraft:diorite",4 },
			{"minecraft:granite",4 },
			{"minecraft:andesite",4 },
			{"minecraft:gravel",4 },
			{"minecraft:oak_log",5 },
			{"minecraft:birch_log",5 },
			{"minecraft:spruce_log",5 },
			{"minecraft:jungle_log",5 },
			{"minecraft:acacia_log",5 },
			{"minecraft:dark_oak_log",5 },
			{"minecraft:snow",6 },
			{"minecraft:snow_block",6 },
			{"minecraft:sand",7 },
			{"minecraft:sandstone",7 },
			{"minecraft:cobblestone",8 },
			{"minecraft:bedrock",8 },
			{"minecraft:obsidian", 8 },
			{"minecraft:lava",9 },
			{"minecraft:magma_block",9 }
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

		static Color[,] colormap;

		public static string GetRandomColor(Random r)
		{
			return commonColors[r.Next(commonColors.Length)];
		}

		public static bool IsBlockForMap(BlockState b, HeightmapType type) {
			if(b == null || IsAir(b)) return false;
			if(type == HeightmapType.AllBlocks) {
				return true;
			} else if(type == HeightmapType.SolidBlocks) {
				return !IsTransparentBlock(b);
			} else if(type == HeightmapType.SolidBlocksNoLiquid) {
				return !IsTransparentBlock(b) && !IsLiquid(b);
			} else if(type == HeightmapType.TerrainBlocks) {
				return b.CompareMultiple(commonTerrainBlocks) || b.CompareMultiple(waterBlock, lavaBlock);
			} else if(type == HeightmapType.TerrainBlocksNoLiquid) {
				return b.CompareMultiple(commonTerrainBlocks);
			} else {
				return false;
			}
		}

		public static Color GetMapColor(string block, int shade) {
			if(colormap == null) {
				colormap = LoadColorMap();
			}
			if(block != null) {
				if (!block.Contains(":")) block = "minecraft:" + block;
				if(!colorMapIndices.TryGetValue(block, out int index)) {
					index = 15;
				}
				shade = 1 - shade;
				return colormap[index, shade];
			} else {
				return Color.FromArgb(0, 0, 0, 0);
			}
		}

		static Color[,] LoadColorMap() {
			Bitmap bmp = new Bitmap(Image.FromStream(new MemoryStream(Resources.colormap)));
			Color[,] colormap = new Color[bmp.Width, 3];
			for(int x = 0; x < bmp.Width; x++) {
				for(int y = 0; y < 3; y++) {
					colormap[x, y] = bmp.GetPixel(x, y);
				}
			}
			return colormap;
		}

		public static bool IsAir(BlockState b) {
			return b.CompareMultiple("minecraft:air", "minecraft:cave_air");
		}

		public static bool IsLiquid(BlockState b) {
			return b.CompareMultiple(waterBlock, lavaBlock);
		}

		public static bool IsPlantSustaining(BlockState b)
		{
			return b.CompareMultiple(plantSustainingBlocks);
		}

		public static bool IsTransparentBlock(BlockState bs) {
			string b = bs.ID;
			if(b == null) return true;
			b = b.Replace("minecraft:", "");
			if(b.Contains("glass")) return true;
			if(b.Contains("bars")) return true;
			if(b.Contains("sapling")) return true;
			if(b.Contains("rail")) return true;
			if(b.Contains("tulip")) return true;
			if(b.Contains("mushroom")) return true;
			if(b.Contains("pressure_plate")) return true;
			if(b.Contains("button")) return true;
			if(b.Contains("torch")) return true;
			if(b.Contains("fence")) return true;
			if(b.Contains("door")) return true;
			if(b.Contains("carpet")) return true;
			switch(b) {
				case "air":
				case "cave:air":
				case "cobweb":
				case "grass":
				case "fern":
				case "dead_bush":
				case "seagrass":
				case "sea_pickle":
				case "dandelion":
				case "poppy":
				case "blue_orchid":
				case "allium":
				case "azure_bluet":
				case "end_rod":
				case "ladder":
				case "lever":
				case "snow_layer":
				case "lily_pad":
				case "tripwire_hook":
				case "barrier":
				case "tall_grass":
				case "large_fern":
				case "sunflower":
				case "lilac":
				case "rose_bush":
				case "peony":
				case "structure_void":
				case "turtle_egg":
				case "redstone":
				case "sweet_berry_bush": return true;
			}
			return false;
		}
	}
}

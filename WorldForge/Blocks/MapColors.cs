using System.Collections.Generic;
using System;
using System.Linq;

namespace WorldForge
{
	public static class MapColors
	{
		public class ColorTone
		{
			public readonly string name;
			public readonly BitmapColor baseColor;
			public readonly BitmapColor shade1;
			public readonly BitmapColor shade2;
			public readonly BitmapColor shade3;

			public ColorTone(string name, BitmapColor baseColor)
			{
				this.name = name;
				this.baseColor = baseColor;
				shade1 = Shade(this.baseColor, 220);
				shade2 = Shade(this.baseColor, 180);
				shade3 = Shade(this.baseColor, 135);
			}

			public ColorTone(string name, string baseColor)
			{
				this.name = name;
				var channels = baseColor.Split(',').Select(byte.Parse).ToArray();
				if(channels.Length > 3)
				{
					this.baseColor = new BitmapColor(channels[0], channels[1], channels[2], channels[3]);
				}
				else
				{
					this.baseColor = new BitmapColor(channels[0], channels[1], channels[2]);
				}
				shade1 = Shade(this.baseColor, 220);
				shade2 = Shade(this.baseColor, 180);
				shade3 = Shade(this.baseColor, 135);
			}

			private static BitmapColor Shade(BitmapColor c, byte multiplier)
			{
				byte r = (byte)(c.r / 255f * multiplier);
				byte g = (byte)(c.g / 255f * multiplier);
				byte b = (byte)(c.b / 255f * multiplier);
				return new BitmapColor(r, g, b, 255);
			}

			public BitmapColor this[int i]
			{
				get
				{
					switch(i)
					{
						case 0: return baseColor;
						case 1: return shade1;
						case 2: return shade2;
						case 3: return shade3;
						default: throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		public static readonly Dictionary<NamespacedID, int> colorMapIndices = new Dictionary<NamespacedID, int>
		{
			{new NamespacedID("grass_block"),       1 },
			{new NamespacedID("dirt"),             10 },
			{new NamespacedID("coarse_dirt"),      10 },
			{new NamespacedID("podzol"),           34 },
			{new NamespacedID("water"),            12 },
			{new NamespacedID("oak_leaves"),        7 },
			{new NamespacedID("birch_leaves"),      7 },
			{new NamespacedID("spruce_leaves"),     7 },
			{new NamespacedID("jungle_leaves"),     7 },
			{new NamespacedID("acacia_leaves"),     7 },
			{new NamespacedID("dark_oak_leaves"),   7 },
			{new NamespacedID("azalea_leaves"),     7 },
			{new NamespacedID("mangrove_leaves"),   7 },
			{new NamespacedID("cactus"),            7 },
			{new NamespacedID("short_grass"),       7 },
			{new NamespacedID("tall_grass"),        7 },
			{new NamespacedID("dandelion"),         7 },
			{new NamespacedID("poppy"),             7 },
			{new NamespacedID("stone"),            11 },
			{new NamespacedID("diorite"),          14 },
			{new NamespacedID("granite"),          10 },
			{new NamespacedID("andesite"),         11 },
			{new NamespacedID("gravel"),           11 },
			{new NamespacedID("oak_log"),          13 },
			{new NamespacedID("birch_log"),         2 },
			{new NamespacedID("spruce_log"),       34 },
			{new NamespacedID("jungle_log"),       10 },
			{new NamespacedID("acacia_log"),       15 },
			{new NamespacedID("dark_oak_log"),     26 },
			{new NamespacedID("snow"),              8 },
			{new NamespacedID("snow_block"),        8 },
			{new NamespacedID("sand"),              2 },
			{new NamespacedID("sandstone"),         2 },
			{new NamespacedID("cobblestone"),      11 },
			{new NamespacedID("bedrock"),          11 },
			{new NamespacedID("obsidian"),         29 },
			{new NamespacedID("lava"),              4 },
			{new NamespacedID("magma_block"),      35 },
			{new NamespacedID("ice"),               5 },
			{new NamespacedID("packed_ice"),        5 },
			{new NamespacedID("blue_ice"),          5 },
			{new NamespacedID("frosted_ice"),       5 }
		};

		private static ColorTone invalidColor = new ColorTone("INVALID", new BitmapColor(255, 0, 255));
		private static ColorTone[] mapColorPalette;

		private static void InitializeColorMap()
		{
			var text = WorldForgeManager.GetResourceAsText("mapcolors.csv");
			var csv = new CSV(text);
			mapColorPalette = new ColorTone[csv.data.Count];
			for(int i = 0; i < csv.data.Count; i++)
			{
				var line = csv.data[i];
				mapColorPalette[i] = new ColorTone(line[1], line[2]);
			}
		}

		public static BitmapColor GetColor(BlockID block, int shade)
		{
			if(mapColorPalette == null)
			{
				InitializeColorMap();
			}
			if(block != null)
			{
				shade = 1 - shade;
				if(!colorMapIndices.TryGetValue(block.ID, out int index))
				{
					return invalidColor[shade];
				}
				return mapColorPalette[index][shade];
			}
			else
			{
				return new BitmapColor(0, 0, 0, 0);
			}
		}
	}
}
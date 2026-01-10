using System.Linq;
using System;
using System.Collections.Generic;

namespace WorldForge.Maps
{
	public class MapColorPalette
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

			public BitmapColor[] GetColors()
			{
				return new BitmapColor[]
				{
					baseColor,
					shade1,
					shade2,
					shade3
				};
			}
		}

		public static readonly ColorTone InvalidTone = new ColorTone("INVALID", new BitmapColor(255, 0, 255));
		public static MapColorPalette Modern { get; private set; }
		public static MapColorPalette Legacy112 { get; private set; }
		public static MapColorPalette Legacy181 { get; private set; }
		public static MapColorPalette Legacy172 { get; private set; }
		public static MapColorPalette Original { get; private set; }

		public Dictionary<NamespacedID, int> mappings = new Dictionary<NamespacedID, int>();
		public ColorTone[] mapColorPalette;
		
		public List<NamespacedID> reportedMissingBlocks = new List<NamespacedID>();

		public static void InitializePalettes()
		{
			Logger.Verbose("Initializing map color palettes ...");
			Modern = new MapColorPalette("mapcolors_modern.csv");
			//TODO: make the other block palettes
			/*
			Legacy112 = new MapColorPalette("mapcolors_1.12.csv");
			Legacy181 = new MapColorPalette("mapcolors_1.8.1.csv");
			Legacy172 = new MapColorPalette("mapcolors_1.7.2.csv");
			Original = new MapColorPalette("mapcolors_original.csv");
			*/
		}

		internal MapColorPalette(string resourceName)
		{
			List<string> resolvedBlocks = new List<string>();
			var text = WorldForgeManager.GetResourceAsText(resourceName);
			var csv = new CSV(text);
			mapColorPalette = new ColorTone[csv.data.Count];
			for(int i = 0; i < csv.data.Count; i++)
			{
				var line = csv.data[i];
				mapColorPalette[i] = new ColorTone(line[1], line[2]);
				if(line.ColumnCount > 3)
				{
					var blocks = line[3].Trim().Split(',');
					foreach(var b in blocks)
					{
						if(string.IsNullOrWhiteSpace(b)) continue;
						HandleWildcards(b, resolvedBlocks);
						foreach(var r in resolvedBlocks)
						{
							var resolvedBlockId = BlockList.Find(r, false, false);
							if(resolvedBlockId != null)
							{
								mappings.Add(resolvedBlockId.ID, i);
							}
							else
							{
								Logger.Warning($"Unknown block '{r}' from '{b}'");
							}
						}
					}
				}
			}
		}

		private void HandleWildcards(string input, List<string> blocks)
		{
			blocks.Clear();
			if (input.EndsWith("W"))
			{
				//Wood variant blocks excluding logs
				input = input.Substring(0, input.Length - 1);
				blocks.Add(input + "_planks");
				blocks.Add("stripped_"+ input + "_log");
				blocks.Add("stripped_"+ input + "_wood");
				blocks.Add(input + "_sign");
				blocks.Add(input + "_wall_sign");
				blocks.Add(input + "_door");
				blocks.Add(input + "_pressure_plate");
				blocks.Add(input + "_fence");
				blocks.Add(input + "_fence_gate");
				blocks.Add(input + "_trapdoor");
				blocks.Add(input + "_stairs");
				blocks.Add(input + "_slab");
			}
			else if (input.EndsWith("B"))
			{
				//Colorful blocks
				input = input.Substring(0, input.Length - 1);
				blocks.Add(input + "_wool");
				blocks.Add(input + "_carpet");
				blocks.Add(input + "_shulker_box");
				blocks.Add(input + "_bed");
				blocks.Add(input + "_stained_glass");
				blocks.Add(input + "_stained_glass_pane");
				blocks.Add(input + "_glazed_terracotta");
				blocks.Add(input + "_concrete");
				blocks.Add(input + "_concrete_powder");
				blocks.Add(input + "_candle");
			}
			else if (input.EndsWith("V"))
			{
				//Variant blocks including self
				input = input.Substring(0, input.Length - 1);
				var self = input;
				if (self.EndsWith("brick")) self += "s";
				blocks.Add(self);
				blocks.Add(input + "_stairs");
				blocks.Add(input + "_slab");
				blocks.Add(input + "_wall");
			}
			else if (input.EndsWith("O"))
			{
				//Stone and ore variants
				input = input.Substring(0, input.Length - 1);
				blocks.Add(input);
				blocks.Add(input + "_coal_ore");
				blocks.Add(input + "_iron_ore");
				blocks.Add(input + "_gold_ore");
				blocks.Add(input + "_copper_ore");
				blocks.Add(input + "_diamond_ore");
				blocks.Add(input + "_emerald_ore");
				blocks.Add(input + "_redstone_ore");
				blocks.Add(input + "_lapis_ore");
			}
			else if (input.EndsWith("COPPER"))
			{
				//Copper variants
				input = input.Substring(0, input.Length - 6);
				if (input.Length == 0)
				{
					blocks.Add("copper_block");
					blocks.Add("waxed_copper_block");
				}
				else
				{
					blocks.Add(input + "_copper");
					blocks.Add("waxed_" + input + "_copper");
				}
				if (input.Length > 0) input += "_";
				blocks.Add(input + "cut_copper");
				blocks.Add("waxed_" + input + "cut_copper");
				blocks.Add(input + "cut_copper_stairs");
				blocks.Add("waxed_" + input + "cut_copper_stairs");
				blocks.Add(input + "cut_copper_slab");
				blocks.Add("waxed_" + input + "cut_copper_slab");
				blocks.Add(input + "copper_trapdoor");
				blocks.Add("waxed_" + input + "copper_trapdoor");
				blocks.Add(input + "copper_bulb");
				blocks.Add("waxed_" + input + "copper_bulb");
				blocks.Add(input + "chiseled_copper");
				blocks.Add("waxed_" + input + "chiseled_copper");
			}
			else
			{
				blocks.Add(input);
			}
		}

		public int GetIndex(NamespacedID id)
		{
			if(mappings.TryGetValue(id, out int index))
			{
				return index;
			}
			if (!reportedMissingBlocks.Contains(id))
			{
				reportedMissingBlocks.Add(id);
				Logger.Warning("No color mapping for block: " + id);
			}
			return -1;
		}

		public BitmapColor GetColor(BlockID block, int shade)
		{
			if(block == null)
			{
				return new BitmapColor(0, 0, 0, 0);
			}
			shade = 1 - shade;
			var index = GetIndex(block.ID);
			if(index == -1)
			{
				return InvalidTone[shade];
			}
			return mapColorPalette[index][shade];
		}
	}
}
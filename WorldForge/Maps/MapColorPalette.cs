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
		}

		public static readonly ColorTone InvalidTone = new ColorTone("INVALID", new BitmapColor(255, 0, 255));
		public static MapColorPalette Modern { get; private set; }
		public static MapColorPalette Legacy112 { get; private set; }
		public static MapColorPalette Legacy181 { get; private set; }
		public static MapColorPalette Legacy172 { get; private set; }
		public static MapColorPalette Original { get; private set; }

		public Dictionary<NamespacedID, int> mappings = new Dictionary<NamespacedID, int>();
		public ColorTone[] mapColorPalette;

		public static void InitializePalettes()
		{
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
			var text = WorldForgeManager.GetResourceAsText(resourceName);
			var csv = new CSV(text);
			mapColorPalette = new ColorTone[csv.data.Count];
			for(int i = 0; i < csv.data.Count; i++)
			{
				var line = csv.data[i];
				mapColorPalette[i] = new ColorTone(line[1], line[2]);
				if(line.ColumnCount > 3)
				{
					var patterns = line[3].Trim().Split(',');
					foreach(var p in patterns)
					{
						if(string.IsNullOrWhiteSpace(p)) continue;
						if(p.Contains("*"))
						{
							foreach(var b in BlockList.Search(p, false, true))
							{
								mappings.Add(b.ID, i);
							}
						}
						else
						{
							mappings.Add(BlockList.Find(p, true).ID, i);
						}
					}
				}
			}
		}

		public int GetIndex(NamespacedID id)
		{
			if(mappings.TryGetValue(id, out int index))
			{
				return index;
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
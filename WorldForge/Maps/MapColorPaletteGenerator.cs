using System.IO.Compression;

namespace WorldForge.Maps
{
	public static class MapColorPaletteGenerator
	{
		private class BiomeTints
		{
			public BitmapColor grass;
			public BitmapColor foliage;
			public BitmapColor dryFoliage;
		}

		public static MapColorPalette CreateFromMinecraftJar(string jarPath, MapColorPalette fallback)
		{
			MapColorPalette palette = new MapColorPalette();
			BiomeTints tints = new BiomeTints();
			using (var jar = new ZipArchive(System.IO.File.OpenRead(jarPath)))
			{
				tints.grass = GetColorTint(jar, "grass", new BitmapColor(101, 196, 74));
				tints.foliage = GetColorTint(jar, "foliage", new BitmapColor(76, 177, 46));
				tints.dryFoliage = GetColorTint(jar, "dry_grass", new BitmapColor(163, 109, 70));
				var blocks = BlockList.allBlocks;
				foreach (var block in blocks.Values)
				{
					if (block.ID.HasCustomNamespace) continue; // Skip modded blocks
					var baseBlock = GetBaseBlock(block);
					if (palette.mappings.ContainsKey(baseBlock.ID))
					{
						// Already mapped
						if(block.ID != baseBlock.ID)
						{
							var index = palette.mappings[baseBlock.ID];
							palette.mappings.Add(block.ID, index);
						}
						continue;
					}
					var id = baseBlock.ID;
					var avg = GetAverageColor(jar, id);
					BitmapColor? color;
					if (avg != null)
					{
						color = avg.Value;
						if (RequiresTint(baseBlock.ID, tints, out var tint))
						{
							color = color.Value.Multiply(tint);
						}
						// Manually increase brightness of grass blocks and water to improve visibility on the map
						if (baseBlock.ID.id == "grass_block")
						{
							color = color.Value.MultiplyBrightness(1.2f);
						}
						else if (baseBlock.ID.id == "water")
						{
							color = color.Value.MultiplyBrightness(1.1f);
						}
					}
					else
					{
						color = fallback?.GetColor(baseBlock, 0);
					}
					if(color != null) palette.Add(color.Value, id);
				}
			}
			return palette;
		}

		private static BlockID GetBaseBlock(BlockID block)
		{
			string original = block.ID.id;
			string id = GetBaseBlockID(block.ID.id);
			var baseBlock = BlockList.Find(id);
			return baseBlock ?? block;
		}

		private static string GetBaseBlockID(string id)
		{
			if (id.EndsWith("_carpet"))
			{
				id.Replace("_carpet", "_wool");
				return id;
			}
			TryTrimPrefix(ref id, "waxed_");

			TryTrimSuffix(ref id, "_stairs");
			TryTrimSuffix(ref id, "_slab");
			TryTrimSuffix(ref id, "_wall");
			TryTrimSuffix(ref id, "_fence_gate");
			TryTrimSuffix(ref id, "_fence");
			TryTrimSuffix(ref id, "_button");
			TryTrimSuffix(ref id, "_pressure_plate");
			TryTrimSuffix(ref id, "_wall_hanging_sign");
			TryTrimSuffix(ref id, "_hanging_sign");
			TryTrimSuffix(ref id, "_wall_sign");
			TryTrimSuffix(ref id, "_sign");

			if (id.EndsWith("brick")) id += "s";

			switch(id)
			{
				case "oak":
				case "spruce":
				case "birch":
				case "jungle":
				case "acacia":
				case "dark_oak":
				case "mangrove":
				case "cherry":
				case "crimson":
				case "warped":
				case "pale_oak":
					id += "_planks";
					break;
			}
			return id;
		}

		private static bool TryTrimPrefix(ref string str, string prefix)
		{
			if (str.StartsWith(prefix))
			{
				str = str.Substring(prefix.Length);
				return true;
			}
			return false;
		}

		private static bool TryTrimSuffix(ref string str, string suffix)
		{
			if (str.EndsWith(suffix))
			{
				str = str.Substring(0, str.Length - suffix.Length);
				return true;
			}
			return false;
		}

		private static bool RequiresTint(NamespacedID id, BiomeTints tints, out BitmapColor tint)
		{
			switch(id.id)
			{
				case "grass_block":
					tint = tints.grass;
					return true;
				case "short_grass":
				case "tall_grass":
				case "fern":
				case "large_fern":
					tint = tints.grass;
					return true;
				case "vines":
				case "oak_leaves":
				case "jungle_leaves":
				case "acacia_leaves":
				case "dark_oak_leaves":
				case "mangrove_leaves":
					tint = tints.foliage;
					return true;
				case "leaf_litter":
					tint = tints.dryFoliage;
					return true;
				case "spruce_leaves":
					tint = new BitmapColor(97, 153, 97); // Hardcoded by Minecraft
					return true;
				case "birch_leaves":
					tint = new BitmapColor(128, 167, 85); // Hardcoded by Minecraft
					return true;
				default:
					tint = default;
					return false;
			}
		}

		private static BitmapColor? GetAverageColor(ZipArchive jar, NamespacedID block)
		{
			var entry = GetTextureEntry(jar, block);
			if (entry != null)
			{
				using (var stream = entry.Open())
				{
					var bitmap = Bitmaps.LoadFromStream(stream);
					int totalR = 0;
					int totalG = 0;
					int totalB = 0;
					float totalPixels = 0f;
					for (int x = 0; x < bitmap.Width; x++)
					{
						for (int y = 0; y < bitmap.Height; y++)
						{
							var c = bitmap.GetPixel(x, y);
							if (c.a < 8) continue; // Ignore mostly transparent pixels
							float a = c.a / 255f;
							totalR += (int)(c.r * a);
							totalG += (int)(c.g * a);
							totalB += (int)(c.b * a);
							totalPixels += a;
						}
					}
					byte avgR = (byte)(totalR / totalPixels);
					byte avgG = (byte)(totalG / totalPixels);
					byte avgB = (byte)(totalB / totalPixels);
					return new BitmapColor(avgR, avgG, avgB);
				}
			}
			return null;
		}

		private static ZipArchiveEntry GetTextureEntry(ZipArchive jar, NamespacedID block)
		{
			const string basePath = "assets/minecraft/textures/block/";
			if (TryGetEntry(jar, basePath + block.id + ".png", out var e)) return e;
			if (TryGetEntry(jar, basePath + block.id + "_top.png", out e)) return e;
			if (TryGetEntry(jar, basePath + block.id + "_side.png", out e)) return e;
			if (TryGetEntry(jar, basePath + block.id + "_0.png", out e)) return e;
			return null;
		}

		private static BitmapColor GetColorTint(ZipArchive jar, string tintTextureName, BitmapColor fallback)
		{
			if (TryGetEntry(jar, $"assets/minecraft/textures/colormap/{tintTextureName}.png", out var entry))
			{
				using (var stream = entry.Open())
				{
					var bitmap = Bitmaps.LoadFromStream(stream);
					return bitmap.GetPixel(0, 0);
				}
			}
			return fallback;
		}

		private static bool TryGetEntry(ZipArchive jar, string path, out ZipArchiveEntry entry)
		{
			entry = jar.GetEntry(path);
			return entry != null;
		}
	}
}
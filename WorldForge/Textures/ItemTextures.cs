using System.Collections.Generic;
using System.IO.Compression;
using WorldForge.Items;

namespace WorldForge.Textures
{
	public class ItemTextures
	{
		public readonly Dictionary<NamespacedID, IBitmap> itemTextures = new Dictionary<NamespacedID, IBitmap>();
		public readonly Dictionary<NamespacedID, IBitmap> blockTextures = new Dictionary<NamespacedID, IBitmap>();

		public static ItemTextures CreateFomMinecraftJar(string jarPath, IEnumerable<BlockID> blocks, IEnumerable<ItemID> items)
		{
			var textures = new ItemTextures();
			using (var jar = new ZipArchive(System.IO.File.OpenRead(jarPath)))
			{
				if(blocks != null)
				{
					foreach (var block in blocks)
					{
						if(block.ID.HasCustomNamespace) continue;
						var entry = jar.GetEntry($"assets/minecraft/textures/block/{block.ID.id}.png");
						if (entry != null)
						{
							using (var stream = entry.Open()) textures.blockTextures[block.ID] = Bitmaps.LoadFromStream(stream);
						}
					}
				}
				if (items != null)
				{
					foreach (var item in items)
					{
						if(item.ID.HasCustomNamespace) continue;
						var entry = jar.GetEntry($"assets/minecraft/textures/item/{item.ID.id}.png");
						if (entry != null)
						{
							using (var stream = entry.Open()) textures.itemTextures[item.ID] = Bitmaps.LoadFromStream(stream);
						}
					}
				}
			}
			return textures;
		}

		public IBitmap GetTexture(NamespacedID id)
		{
			if (blockTextures.TryGetValue(id, out var blockTexture)) return blockTexture;
			if (itemTextures.TryGetValue(id, out var itemTexture)) return itemTexture;
			return null;
		}

		public bool TryGetTexture(NamespacedID id, out IBitmap texture)
		{
			texture = GetTexture(id);
			return texture != null;
		}
	}
}
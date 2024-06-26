using System.Collections.Generic;

namespace WorldForge.Items
{
	public class ItemID
	{
		public string ID => (customNamespace ?? "minecraft") + ":" + shortID;

		public GameVersion AddedInVersion { get; private set; } = GameVersion.FirstVersion;

		public readonly string customNamespace = null;
		public readonly string shortID;

		public readonly NumericID? numericID = null;

		public ItemID substitute;

		public static void ParseID(string fullID, out string namespaceName, out string shortID)
		{
			var split = fullID.Split(':');
			if(split.Length == 1)
			{
				namespaceName = "minecraft";
				shortID = split[0];
			}
			else
			{
				namespaceName = split[0];
				shortID = split[1];
			}
		}

		internal static ItemID Get(string id)
		{
			if(!id.Contains(":")) id = "minecraft:" + id;
			if(ItemList.allItems.TryGetValue(id, out var b)) return b;
			else return null;
		}

		public static ItemID RegisterVanillaItem(string shortID, GameVersion? versionAdded = null, ItemID substitute = null, NumericID? numericID = null)
		{
			return RegisterNewItem(null, shortID, versionAdded, substitute, numericID);
		}

		public static ItemID RegisterModdedItem(string modNamespace, string shortID)
		{
			return RegisterNewItem(modNamespace, shortID, null, null);
		}

		private static ItemID RegisterNewItem(string customNamespace, string shortID, GameVersion? versionAdded, ItemID substitute, NumericID? numericID = null)
		{
			if(!versionAdded.HasValue) versionAdded = GameVersion.FirstVersion;
			var b = new ItemID(customNamespace, shortID, versionAdded.Value, substitute, numericID);
			ItemList.allItems.Add(b.ID, b);
			return b;
		}

		public ItemID(string ns, string id, GameVersion v, ItemID sub, NumericID? num)
		{
			customNamespace = ns;
			shortID = id;
			AddedInVersion = v;
			substitute = sub;
			numericID = num;
		}

		public static void ResolveItemID(GameVersion version, ref ItemID item)
		{
			int i = 0;
			while(item != null && version < item.AddedInVersion)
			{
				if(i > 100) throw new System.Exception("Infinite loop in item ID resolution.");
				item = item.substitute;
				i++;
			}
		}
	}
}

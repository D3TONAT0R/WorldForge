namespace WorldForge.Items
{
	public class ItemID
	{
		public readonly NamespacedID ID;
		public readonly NumericID? numericID = null;

		public GameVersion AddedInVersion { get; private set; } = GameVersion.FirstVersion;
		public ItemID substitute;

		public static ItemID Get(string id)
		{
			if(!id.Contains(":")) id = "minecraft:" + id;
			if(ItemList.allItems.TryGetValue(id, out var b)) return b;
			else return null;
		}

		public static ItemID RegisterVanillaItem(NamespacedID id, GameVersion? versionAdded = null, ItemID substitute = null, NumericID? numericID = null)
		{
			return RegisterNewItem(id, versionAdded, substitute, numericID);
		}

		public static ItemID RegisterModdedItem(string modNamespace, string id)
		{
			if(modNamespace == null) throw new System.Exception("Modded items must have a namespace.");
			if(modNamespace == "minecraft") throw new System.Exception("Modded items cannot be in the minecraft namespace.");
			return RegisterNewItem(new NamespacedID(modNamespace, id), null, null);
		}

		private static ItemID RegisterNewItem(NamespacedID id, GameVersion? versionAdded, ItemID substitute, NumericID? numericID = null)
		{
			if(!versionAdded.HasValue) versionAdded = GameVersion.FirstVersion;
			var b = new ItemID(id, versionAdded.Value, substitute, numericID);
			ItemList.allItems.Add(b.ID, b);
			return b;
		}

		public ItemID(NamespacedID id, GameVersion v, ItemID sub, NumericID? num)
		{
			ID = id;
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

		public override int GetHashCode() => ID.GetHashCode();
	}
}

namespace WorldForge.Items
{
	public class ItemID
	{
		public string ID => (customNamespace ?? "minecraft") + ":" + shortID;

		public GameVersion AddedInVersion { get; private set; } = GameVersion.FirstVersion;

		public readonly string customNamespace = null;
		public readonly string shortID;

		public ItemID substitute;

		public static ItemID Get(string id)
		{
			if(ItemList.allItems.TryGetValue(id, out var b)) return b;
			return null;
		}

		public static ItemID RegisterVanillaItem(string shortID, GameVersion? versionAdded = null, ItemID substitute = null)
		{
			return RegisterNewItem(null, shortID, versionAdded, substitute);
		}

		public static ItemID RegisterModdedItem(string modNamespace, string shortID)
		{
			return RegisterNewItem(modNamespace, shortID, null, null);
		}

		private static ItemID RegisterNewItem(string customNamespace, string shortID, GameVersion? versionAdded, ItemID substitute)
		{
			if(!versionAdded.HasValue) versionAdded = GameVersion.FirstVersion;
			var b = new ItemID(customNamespace, shortID, versionAdded.Value, substitute);
			ItemList.allItems.Add(b.ID, b);
			return b;
		}

		public ItemID(string ns, string id, GameVersion v, ItemID sub)
		{
			customNamespace = ns;
			shortID = id;
			AddedInVersion = v;
			substitute = sub;
		}
	}
}

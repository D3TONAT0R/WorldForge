using System.Collections.Generic;

namespace WorldForge.Items
{
	public static class ItemList
	{
		public static Dictionary<string, ItemID> allItems;
		public static Dictionary<ItemID, NumericID> numerics;
		public static Dictionary<NumericID, ItemID> itemByNumerics;
		public static Dictionary<ItemID, string> preFlatteningIDs;

		public static void Initialize(string blockData)
		{
			allItems = new Dictionary<string, ItemID>();
			numerics = new Dictionary<ItemID, NumericID>();
			itemByNumerics = new Dictionary<NumericID, ItemID>();
			preFlatteningIDs = new Dictionary<ItemID, string>();
			
			var csv = new CSV(blockData.Replace("\r","").Split('\n'));
			foreach(var row in csv.data)
			{
				//ID;Numeric ID;Pre-flattening ID;Added in Version;Fallback
				SplitItemID(row[0], out var customNamespace, out var itemId);
				var numeric = NumericID.TryParse(row[1]);
				var preFlatteningId = row[2];
				var version = !string.IsNullOrEmpty(row[3]) ? GameVersion.Parse(row[3]) : GameVersion.FirstVersion;
				ItemID fallback = !string.IsNullOrEmpty(row[4]) ? Find(row[4], true) : null;

				var item = new ItemID(customNamespace, itemId, version, fallback, numeric);
				allItems.Add(item.ID, item);
				if(numeric.HasValue)
				{
					numerics.Add(item, numeric.Value);
					itemByNumerics.Add(numeric.Value, item);
				}
				if(!string.IsNullOrEmpty(preFlatteningId))
				{
					preFlatteningIDs.Add(item, preFlatteningId);
				}
			}
		}

		public static ItemID Find(string itemID, bool throwErrorIfNotFound = false)
		{
			if(!itemID.Contains(":")) itemID = "minecraft:" + itemID;
			if(itemID.StartsWith("minecraft:"))
			{
				if(allItems.TryGetValue(itemID, out var item))
				{
					return item;
				}
				else
				{
					//Try to find block item
					var block = BlockList.Find(itemID, false);
					if(block != null)
					{
						return block;
					}
					else
					{
						if(throwErrorIfNotFound) throw new KeyNotFoundException($"Unable to find an item with name '{itemID}'.");
						return null;
					}
				}
			}
			else
			{
				//Modded block, add it to the list if we haven't done so already.
				if(allItems.TryGetValue(itemID, out var pb))
				{
					return pb;
				}
				else
				{
					//Try to find block item
					var block = BlockList.Find(itemID, false);
					if(block != null)
					{
						return block;
					}
					else
					{
						var split = itemID.Split(':');
						return ItemID.RegisterModdedItem(split[0], split[1]);
					}
				}
			}
		}

		public static ItemID FindByNumeric(NumericID numeric, bool throwErrorIfNotFound = false)
		{
			if(itemByNumerics.TryGetValue(numeric, out var itemId))
			{
				return itemId;
			}
			else if(itemByNumerics.TryGetValue(numeric.WithoutDamage, out itemId))
			{
				return itemId;
			}
			else
			{
				if(throwErrorIfNotFound) throw new KeyNotFoundException($"Unable to find item definition with numeric ID '{numeric}'.");
				return null;
			}
		}

		private static void SplitItemID(string fullId, out string customNamespaceName, out string itemID)
		{
			var split = fullId.Split(':');
			if(split.Length > 1)
			{
				customNamespaceName = split[0];
				if(customNamespaceName == "minecraft") customNamespaceName = null;
				itemID = split[1];
			}
			else
			{
				customNamespaceName = null;
				itemID = split[0];
			}
		}
	}
}

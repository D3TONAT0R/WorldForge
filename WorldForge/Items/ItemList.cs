using System.Collections.Generic;

namespace WorldForge.Items
{
	public static class ItemList
	{
		public static Dictionary<NamespacedID, ItemID> allItems;
		public static Dictionary<ItemID, NumericID> numerics;
		public static Dictionary<NumericID, ItemID> itemByNumerics;
		public static Dictionary<ItemID, string> preFlatteningIDs;

		public static void Initialize(string blockData)
		{
			Logger.Verbose("Initializing item list ...");
			allItems = new Dictionary<NamespacedID, ItemID>();
			numerics = new Dictionary<ItemID, NumericID>();
			itemByNumerics = new Dictionary<NumericID, ItemID>();
			preFlatteningIDs = new Dictionary<ItemID, string>();
			
			var csv = new CSV(blockData);
			foreach(var row in csv.data)
			{
				//ID;Numeric ID;Pre-flattening ID;Added in Version;Fallback
				var id = new NamespacedID(row[0]);
				var numeric = NumericID.TryParse(row[1]);
				var preFlatteningId = row[2];
				var version = !string.IsNullOrEmpty(row[3]) ? GameVersion.Parse(row[3]) : GameVersion.FirstVersion;
				var fallbackID = new NamespacedID(row[4]);
				ItemID fallback = !string.IsNullOrEmpty(row[4]) ? Find(fallbackID, true) : null;

				var item = new ItemID(id, version, fallback, numeric);
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

		public static ItemID Find(NamespacedID id, bool throwErrorIfNotFound = false)
		{
			if(!id.HasCustomNamespace)
			{
				if(allItems.TryGetValue(id, out var item))
				{
					return item;
				}
				else
				{
					//Try to find block item
					var block = BlockList.Find(id, false);
					if(block != null)
					{
						return block;
					}
					else
					{
						if(throwErrorIfNotFound) throw new KeyNotFoundException($"Unable to find an item with id '{id}'.");
						return null;
					}
				}
			}
			else
			{
				//Modded block, add it to the list if we haven't done so already.
				if(allItems.TryGetValue(id, out var pb))
				{
					return pb;
				}
				else
				{
					//Try to find block item
					var block = BlockList.Find(id, false);
					if(block != null)
					{
						return block;
					}
					else
					{
						return ItemID.RegisterModdedItem(id.customNamespace, id.id);
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
	}
}

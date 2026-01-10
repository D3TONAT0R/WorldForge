using WorldForge.NBT;

namespace WorldForge.Items
{
	//TODO: add support for 1.20.5's new component structure
	public class ItemStack : INBTConverter
	{
		public Item item;
		public byte count;

		public bool IsNull => item == null || count <= 0;

		//Required by Activator.CreateInstance. Do not remove.
		private ItemStack()
		{

		}

		public ItemStack(Item item, byte count)
		{
			this.item = item;
			this.count = count;
		}

		public ItemStack(NamespacedID itemID, byte count) : this(new Item(itemID), count)
		{

		}

		public ItemStack(string itemID, byte count) : this(new Item(new NamespacedID(itemID)), count)
		{

		}

		public ItemStack(NBTCompound nbt, out sbyte slotIndex)
		{
			item = null;
			count = 0;
			FromNBT(nbt);
			if(!nbt.TryGet("Slot", out slotIndex))
			{
				slotIndex = -1;
			}
		}

		public void FromNBT(object nbtData)
		{
			var nbt = (NBTCompound)nbtData;
			if(nbt.ItemCount == 0) return;
			item = new Item(nbt);
			if(!nbt.TryGet<byte>("count", out count))
			{
				count = nbt.Get<byte>("Count");
			}
		}

		public bool ToNBT(sbyte? slotIndex, GameVersion version, out NBTCompound nbt)
		{
			nbt = new NBTCompound();
			//This returns false if the item was resolved to air because of an older version
			bool exists = item.WriteToNBT(nbt, version);
			if(!exists)
			{
				nbt = null;
				return false;
			}

			if (slotIndex.HasValue)
			{
				nbt.Add("Slot", slotIndex.Value);
			}

			nbt.Add(version >= GameVersion.Release_1(20, 5) ? "count" : "Count", count);

			return true;
		}

		public object ToNBT(GameVersion version)
		{
			if(ToNBT(null, version, out var nbt))
			{
				return nbt;
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return $"{count}x {item.id}";
		}
	}
}

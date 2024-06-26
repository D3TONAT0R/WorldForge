using WorldForge.NBT;

namespace WorldForge.Items
{
	public class ItemStack : INBTConverter
	{
		public Item item;
		public sbyte count;

		public bool IsNull => item == null || count <= 0;

		public ItemStack(Item item, sbyte count)
		{
			this.item = item;
			this.count = count;
		}

		public ItemStack(string itemID, sbyte count) : this(new Item(itemID), count)
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
			item = new Item(nbt);
			count = nbt.Get<sbyte>("Count");
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
			nbt.Add("Count", count);

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
	}
}

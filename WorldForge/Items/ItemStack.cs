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

		public NBTCompound ToNBT(sbyte? slotIndex, GameVersion version)
		{
			NBTCompound nbt = new NBTCompound();
			if (slotIndex.HasValue)
			{
				nbt.Add("Slot", slotIndex.Value);
			}
			nbt.Add("Count", count);
			item.WriteToNBT(nbt, version);
			return nbt;
		}

		public object ToNBT(GameVersion version)
		{
			return ToNBT(null, version);
		}
	}
}

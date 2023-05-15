using MCUtils.NBT;

namespace MCUtils
{
	public struct ItemStack
	{
		public Item item;
		public sbyte count;

		public bool IsNull => item == null || count <= 0;

		public ItemStack(Item item, sbyte count)
		{
			this.item = item;
			this.count = count;
		}

		public ItemStack(NBTCompound nbt, out sbyte slotIndex)
		{
			item = new Item(nbt);
			count = nbt.Get<sbyte>("Count");
			slotIndex = nbt.Get<sbyte>("Slot");
		}

		public NBTCompound ToNBT(sbyte? slotIndex, Version version)
		{
			NBTCompound nbt = new NBTCompound();
			if(slotIndex.HasValue)
			{
				nbt.Add("Slot", slotIndex.Value);
			}
			nbt.Add("Count", count);
			item.WriteToNBT(nbt, version);
			return nbt;
		}
	}
}

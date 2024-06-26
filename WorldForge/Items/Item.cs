using System;
using WorldForge.NBT;

namespace WorldForge.Items
{
	public class Item
	{
		[NBT]
		public ItemID id;
		[NBT]
		public NBTCompound metadata;

		public Item(ItemID id, NBTCompound metadata = null)
		{
			this.id = id;
			this.metadata = metadata;
		}

		public Item(string id, NBTCompound metadata = null) : this(ItemList.Find(id), null)
		{

		}

		public Item(NBTCompound nbt)
		{
			var itemId = nbt.Get("id");
			if(itemId is string s)
			{
				id = ItemID.Get(s);
			}
			else if(itemId is short i)
			{
				nbt.TryGet("Damage", out short damage);
				var num = new NumericID(i, damage);
				id = ItemList.FindByNumeric(num);
			}
			else
			{
				throw new NotSupportedException();
			}
			nbt.TryGet<NBTCompound>("tag", out metadata);
		}

		public void WriteToNBT(NBTCompound nbt, GameVersion version)
		{
			//TODO
			NBTConverter.WriteToNBT(this, nbt, version);
		}
	}
}

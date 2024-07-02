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

		public bool WriteToNBT(NBTCompound nbt, GameVersion version)
		{
			NBTCompound dataToWrite = metadata;
			var resolvedId = id;
			ItemID.ResolveItemID(version, ref resolvedId);
			if(resolvedId == null) return false;

			if(version >= GameVersion.FirstFlatteningVersion)
			{
				nbt.Add("id", resolvedId.ID);
				if(metadata != null) nbt.Add("tag", metadata);
			}
			else
			{
				var num = resolvedId.numericID.Value;
				nbt.Add("id", num.id);
				if(dataToWrite != null)
				{
					if(dataToWrite.Contains("Damage"))
					{
						dataToWrite = dataToWrite.Clone();
						object damage = dataToWrite.Get("Damage");
						nbt.Add("Damage", Convert.ToInt16(damage));
						dataToWrite.Remove("Damage");
					}
					else
					{
						nbt.Add("Damage", num.damage);
					}
				}
				else
				{
					nbt.Add("Damage", num.damage);
				}
				if(metadata != null) nbt.Add("tag", metadata);
			}
			return true;
		}
	}
}

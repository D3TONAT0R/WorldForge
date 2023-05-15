using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;

namespace MCUtils.TileEntities
{
	public class TileEntityContainer : TileEntity
	{
		public Dictionary<sbyte, ItemStack> items = new Dictionary<sbyte, ItemStack>();
		public string customName = null;

		public readonly int maxSlotCount;

		public TileEntityContainer(string id, BlockCoord blockPos, int maxSlotCount) : base(id, blockPos)
		{
			this.maxSlotCount = maxSlotCount;
		}

		public TileEntityContainer(NBTCompound compound, int maxSlotCount) : base(compound)
		{
			if(compound.TryTake("Items", out NBTList itemsList))
			{
				items = new Dictionary<sbyte, ItemStack>();
				for(int i = 0; i < itemsList.Length; i++)
				{
					var slotNBT = itemsList.Get<NBTCompound>(i);
					var stack = new ItemStack(slotNBT, out var slotIndex);
					items.Add(slotIndex, stack);
				}
			}
			compound.TryTake("CustomName", out customName);
			this.maxSlotCount = maxSlotCount;
		}

		public bool SetItem(sbyte slotIndex, ItemStack item)
		{
			if(slotIndex >= maxSlotCount)
			{
				throw new IndexOutOfRangeException();
			}
			if(item.IsNull && items.ContainsKey(slotIndex))
			{
				items.Remove(slotIndex);
				return false;
			}
			items[slotIndex] = item;
			return true;
		}

		public bool HasItem(sbyte slotIndex)
		{
			if(items == null) return false;
			return items.ContainsKey(slotIndex) && !items[slotIndex].IsNull;
		}

		public bool RemoveItem(sbyte slotIndex)
		{
			if(slotIndex >= maxSlotCount)
			{
				throw new IndexOutOfRangeException();
			}
			return items.Remove(slotIndex);
		}

		public ItemStack TakeItem(sbyte slotIndex)
		{
			if(slotIndex >= maxSlotCount)
			{
				throw new IndexOutOfRangeException();
			}
			if(!HasItem(slotIndex))
			{
				throw new InvalidOperationException($"No item was present in slot {slotIndex}.");
			}
			var stack = items[slotIndex];
			items.Remove(slotIndex);
			return stack;
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			NBTList itemList = new NBTList(NBTTag.TAG_Compound);
			foreach(var kv in items)
			{
				if(!kv.Value.IsNull)
				{
					itemList.Add(kv.Value.ToNBT(kv.Key, version));
				}
			}
			nbt.Add("Items", itemList);
		}
	}
}

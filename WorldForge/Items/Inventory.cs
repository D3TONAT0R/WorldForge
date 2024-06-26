using System;
using System.Collections.Generic;
using System.Linq;
using WorldForge.NBT;

namespace WorldForge.Items
{
	public class Inventory : INBTConverter
	{
		public Dictionary<sbyte, ItemStack> items = new Dictionary<sbyte, ItemStack>();

		public sbyte? maxSlotCount = null;

		public ItemStack this[sbyte index]
		{
			get => GetItem(index);
			set => SetItem(index, value);
		}

		public Inventory()
		{

		}

		public ItemStack GetItem(sbyte slotIndex)
		{
			if(items.TryGetValue(slotIndex, out var item))
			{
				return item;
			}
			else
			{
				return null;
			}
		}

		public bool SetItem(sbyte slotIndex, ItemStack item)
		{
			if(maxSlotCount.HasValue && slotIndex >= maxSlotCount)
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
			if(maxSlotCount.HasValue && slotIndex >= maxSlotCount)
			{
				throw new IndexOutOfRangeException();
			}
			return items.Remove(slotIndex);
		}

		public ItemStack TakeItem(sbyte slotIndex)
		{
			if(maxSlotCount.HasValue && slotIndex >= maxSlotCount)
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

		public object ToNBT(GameVersion version)
		{
			var list = new NBTList(NBTTag.TAG_Compound);
			var indices = items.Keys.ToList();
			indices.Sort();
			foreach (var index in indices)
			{
				if(items[index].ToNBT(index, version, out var nbt))
				{
					list.Add(nbt);
				}
			}
			return list;
		}

		public void FromNBT(object nbtData)
		{
			var inventoryNBT = (NBTList)nbtData;
			foreach(var itemNBT in inventoryNBT)
			{
				var item = new ItemStack((NBTCompound)itemNBT, out var slot);
				items.Add(slot, item);
			}
		}
	}
}

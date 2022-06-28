using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCUtils
{
	public class Inventory : INBTCompatible
	{
		public struct ItemStack
		{
			public string itemID;
			public byte count;

			public ItemStack(string id, byte amount)
			{
				itemID = id;
				count = amount;
			}

			public NBTCompound CreateNBT(byte slot)
			{
				var comp = new NBTCompound
				{
					{ "Slot", slot },
					{ "id", itemID },
					{ "Count", count }
				};
				return comp;
			}
		}

		public Dictionary<byte, ItemStack> content = new Dictionary<byte, ItemStack>();

		public Inventory()
		{
		
		}

		public NBTList CreateNBT()
		{
			var list = new NBTList(NBTTag.TAG_Compound);
			var indices = content.Keys.ToList();
			indices.Sort();
			foreach (var index in indices)
			{
				list.Add(content[index].CreateNBT(index));
			}
			return list;
		}

		public object GetNBTCompatibleObject()
		{
			return CreateNBT();
		}

		public void ParseFromNBT(object nbtData)
		{
			var inventoryNBT = (NBTList)nbtData;
			for (int i = 0; i < inventoryNBT.Length; i++)
			{
				try
				{
					var stackNBT = inventoryNBT.Get<NBTCompound>(i);
					var slot = stackNBT.Get<byte>("Slot");
					var stack = new ItemStack(stackNBT.Get<string>("id"), stackNBT.Get<byte>("Count"));
					content.Add(slot, stack);
				}
				catch (Exception e)
				{
					throw new ArgumentException($"Failed to parse inventory item at index {i}: {e.Message}");
				}
			}
		}
	}
}

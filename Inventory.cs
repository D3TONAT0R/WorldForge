using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCUtils
{
	public class Inventory
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
				var comp = new NBTCompound();
				comp.Add("Slot", slot);
				comp.Add("id", itemID);
				comp.Add("Count", count);
				return comp;
			}
		}

		public Dictionary<byte, ItemStack> content = new Dictionary<byte, ItemStack>();

		private Inventory() { }

		public static Inventory CreateNew()
		{
			return new Inventory();
		}

		public static Inventory Load(NBTList inventoryNBT)
		{
			var inv = new Inventory();
			for (int i = 0; i < inventoryNBT.Length; i++)
			{
				try
				{
					var stackNBT = inventoryNBT.Get<NBTCompound>(i);
					var slot = stackNBT.Get<byte>("Slot");
					var stack = new ItemStack(stackNBT.Get<string>("id"), stackNBT.Get<byte>("Count"));
					inv.content.Add(slot, stack);
				}
				catch (Exception e)
				{
					throw new ArgumentException($"Failed to parse inventory item at index {i}: {e.Message}");
				}
			}
			return inv;
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
	}
}

using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityCampfire : TileEntity
	{
		[NBT("CookingTimes")]
		public int[] cookingTimes = new int[0];
		[NBT("CookingTotalTimes")]
		public int[] cookingTotalTimes = new int[0];
		//[NBT("Items")]
		public Dictionary<sbyte, ItemStack> items = new Dictionary<sbyte, ItemStack>();

		public override GameVersion AddedInVersion => GameVersion.Release_1(14);

		public TileEntityCampfire(string id) : base(id)
		{
		}

		public TileEntityCampfire(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
			if(compound.TryGet("Items", out NBTList itemsList))
			{
				foreach(NBTCompound itemNBT in itemsList)
				{
					var item = new ItemStack(itemNBT, out var slot);
					items.Add(slot, item);
				}
			}
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(items != null)
			{
				var itemsList = new NBTList(NBTTag.TAG_Compound);
				foreach(var item in items)
				{
					if(item.Value.ToNBT(item.Key, version, out var itemNBT))
					{
						itemsList.Add(itemNBT);
					}
				}
				nbt.Add("Items", itemsList);
			}
		}
	}
}

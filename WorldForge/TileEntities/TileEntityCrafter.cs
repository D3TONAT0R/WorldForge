using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityCrafter : TileEntity
	{
		[NBT("crafting_ticks_remaining")]
		public int craftingTickRemaining = 0;
		[NBT("triggered")]
		public bool triggered;
		[NBT("disabled_slots")]
		public int[] disabledSlots = new int[0];
		[NBT("Items")]
		public Inventory items;
		[NBT("Lock")]
		public string lockItemName = null;
		[NBTCollection]
		public LootTableOptions lootTableOptions = new LootTableOptions();

		public override GameVersion AddedInVersion => GameVersion.Release_1(21);

		public TileEntityCrafter() : base("crafter")
		{

		}

		public TileEntityCrafter(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

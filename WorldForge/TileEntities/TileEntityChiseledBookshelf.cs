using System;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityChiseledBookshelf : TileEntity
	{
		[NBT("Items")]
		public Inventory items;
		[NBT("last_interacted_slot")]
		public int lastInteractedSlot = -1;

		public override GameVersion AddedInVersion => GameVersion.Release_1(19, 3);

		public TileEntityChiseledBookshelf() : base("chiseled_bookshelf")
		{

		}

		public TileEntityChiseledBookshelf(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

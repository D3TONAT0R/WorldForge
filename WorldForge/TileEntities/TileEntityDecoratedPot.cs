using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityDecoratedPot : TileEntity
	{
		[NBT("sherds")]
		public List<string> sherds = new List<string>();
		[NBT("item")]
		public ItemStack item = null;

		[NBTCollection]
		public LootTableOptions LootTableOptions = new LootTableOptions();

		public override GameVersion AddedInVersion => GameVersion.Release_1(19, 4);

		public TileEntityDecoratedPot() : base("decorated_pot")
		{

		}

		public TileEntityDecoratedPot(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

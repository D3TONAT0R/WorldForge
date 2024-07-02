using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntitySkulkCatalyst : TileEntity
	{
		[NBT("cursors")]
		public List<NBTCompound> cursors = new List<NBTCompound>();

		public TileEntitySkulkCatalyst() : base("sculk_catalyst")
		{
			
		}

		public TileEntitySkulkCatalyst(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			
		}
	}
}

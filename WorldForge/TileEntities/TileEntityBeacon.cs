using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBeacon : TileEntity
	{
		[NBT("CustomName")]
		public JSONTextComponent customName = null;

		[NBT("primary_effect")]
		public string primaryEffect = null;
		[NBT("secondary_effect")]
		public string secondaryEffect = null;

		[NBT("Lock")]
		public string lockItemName = null;

		public TileEntityBeacon() : base("beacon")
		{

		}

		public TileEntityBeacon(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

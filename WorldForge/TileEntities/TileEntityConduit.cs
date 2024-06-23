using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityConduit : TileEntity
	{
		[NBT("Target")]
		public UUID target = null;

		public TileEntityConduit() : base("conduit")
		{
			
		}

		public TileEntityConduit(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityConduit : TileEntity
	{
		public TileEntityConduit() : base("conduit")
		{
		}

		public TileEntityConduit(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

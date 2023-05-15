using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityConduit : TileEntity
	{
		public TileEntityConduit(BlockCoord blockPos) : base("conduit", blockPos)
		{
		}

		public TileEntityConduit(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

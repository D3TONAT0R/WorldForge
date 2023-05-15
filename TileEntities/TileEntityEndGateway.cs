using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityEndGateway : TileEntity
	{
		public TileEntityEndGateway(BlockCoord blockPos) : base("end_gateway", blockPos)
		{
		}

		public TileEntityEndGateway(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

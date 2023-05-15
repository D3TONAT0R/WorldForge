using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityLectern : TileEntity
	{
		public TileEntityLectern(BlockCoord blockPos) : base("lectern", blockPos)
		{
		}

		public TileEntityLectern(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

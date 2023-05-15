using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityPiston : TileEntity
	{
		public TileEntityPiston(BlockCoord blockPos) : base("piston", blockPos)
		{
		}

		public TileEntityPiston(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

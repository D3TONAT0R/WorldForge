using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityBeehive : TileEntity
	{
		public TileEntityBeehive(string id, BlockCoord blockPos) : base(id, blockPos)
		{
		}

		public TileEntityBeehive(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

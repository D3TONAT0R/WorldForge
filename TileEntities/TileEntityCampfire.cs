using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityCampfire: TileEntity
	{
		public TileEntityCampfire(string id, BlockCoord blockPos) : base(id, blockPos)
		{
		}

		public TileEntityCampfire(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

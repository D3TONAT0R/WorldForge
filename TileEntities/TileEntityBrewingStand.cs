using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityBrewingStand : TileEntity
	{
		public TileEntityBrewingStand(string id, BlockCoord blockPos) : base(id, blockPos)
		{
		}

		public TileEntityBrewingStand(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

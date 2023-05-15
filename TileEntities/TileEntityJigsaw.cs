using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityJigsaw : TileEntity
	{
		public TileEntityJigsaw(BlockCoord blockPos) : base("jigsaw", blockPos)
		{
		}

		public TileEntityJigsaw(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

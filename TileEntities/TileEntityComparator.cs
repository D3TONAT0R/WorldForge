using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityComparator : TileEntity
	{
		public TileEntityComparator(BlockCoord blockPos) : base("comparator", blockPos)
		{
		}

		public TileEntityComparator(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

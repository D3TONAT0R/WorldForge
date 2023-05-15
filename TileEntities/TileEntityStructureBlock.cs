using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityStructureBlock : TileEntity
	{
		public TileEntityStructureBlock(BlockCoord blockPos) : base("structure_block", blockPos)
		{
		}

		public TileEntityStructureBlock(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

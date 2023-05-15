using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityCommandBlock : TileEntity
	{
		public TileEntityCommandBlock(BlockCoord blockPos) : base("command_block", blockPos)
		{
		}

		public TileEntityCommandBlock(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

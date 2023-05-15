using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityJukebox : TileEntity
	{
		public TileEntityJukebox(BlockCoord blockPos) : base("jukebox", blockPos)
		{
		}

		public TileEntityJukebox(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

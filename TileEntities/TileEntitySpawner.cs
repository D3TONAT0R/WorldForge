using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntitySpawner : TileEntity
	{
		public TileEntitySpawner(BlockCoord blockPos) : base("mob_spawner", blockPos)
		{
		}

		public TileEntitySpawner(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

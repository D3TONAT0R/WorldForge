using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityPiston : TileEntity
	{
		public TileEntityPiston() : base("piston")
		{
		}

		public TileEntityPiston(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{
			throw new NotImplementedException();
		}
	}
}

using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityCampfire : TileEntity
	{
		public TileEntityCampfire(string id) : base(id)
		{
		}

		public TileEntityCampfire(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{
			throw new NotImplementedException();
		}
	}
}

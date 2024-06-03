using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityComparator : TileEntity
	{
		public TileEntityComparator() : base("comparator")
		{
		}

		public TileEntityComparator(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{
			throw new NotImplementedException();
		}
	}
}

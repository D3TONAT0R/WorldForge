using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityChiseledBookshelf : TileEntity
	{
		public TileEntityChiseledBookshelf() : base("chiseled_bookshelf")
		{
		}

		public TileEntityChiseledBookshelf(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{
			throw new NotImplementedException();
		}
	}
}

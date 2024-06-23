using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBed : TileEntity
	{
		public TileEntityBed() : base("bed")
		{

		}

		public TileEntityBed(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

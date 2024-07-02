using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBell : TileEntity
	{
		public TileEntityBell() : base("bell")
		{
			
		}

		public TileEntityBell(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

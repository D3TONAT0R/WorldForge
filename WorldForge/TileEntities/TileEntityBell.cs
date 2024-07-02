using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBell : TileEntity
	{
		public override GameVersion AddedInVersion => GameVersion.Release_1(14);

		public TileEntityBell() : base("bell")
		{
			
		}

		public TileEntityBell(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

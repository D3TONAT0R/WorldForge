using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEndPortal : TileEntity
	{
		public TileEntityEndPortal() : base("end_portal")
		{

		}

		public TileEntityEndPortal(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		public override string ResolveTileEntityID(GameVersion version)
		{
			return version >= GameVersion.Release_1(11) ? id : "AirPortal";
		}
	}
}

using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityDaylightDetector : TileEntity
	{
		public TileEntityDaylightDetector() : base("daylight_detector")
		{

		}

		public TileEntityDaylightDetector(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		public override string ResolveTileEntityID(GameVersion version)
		{
			return version >= GameVersion.Release_1(11) ? id : "DLDetector";
		}
	}
}

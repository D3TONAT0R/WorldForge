using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntitySkulkShrieker : TileEntity
	{
		[NBT("VibrationListener")]
		public NBTCompound vibrationListener;

		public TileEntitySkulkShrieker() : base("sculk_shrieker")
		{

		}

		public TileEntitySkulkShrieker(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}
	}
}

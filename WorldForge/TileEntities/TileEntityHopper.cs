using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityHopper : TileEntityContainer
	{
		[NBT("TransferCooldown")]
		public int transferCooldown = 0;

		public TileEntityHopper() : base("hopper", 5)
		{
		}

		public TileEntityHopper(NBTCompound nbt, out BlockCoord blockPos) : base(nbt, 5, out blockPos)
		{

		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{

		}
	}
}

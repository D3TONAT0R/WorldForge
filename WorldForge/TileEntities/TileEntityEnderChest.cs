using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEnderChest : TileEntity
	{
		public override GameVersion AddedInVersion => GameVersion.Release_1(3, 1);

		public TileEntityEnderChest() : base("ender_chest")
		{

		}

		public TileEntityEnderChest(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override string ResolveTileEntityID(GameVersion version)
		{
			return version >= GameVersion.Release_1(11) ? id : "EnderChest";
		}
	}
}

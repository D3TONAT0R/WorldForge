using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntitySuspiciousBlock : TileEntity
	{
		[NBTCollection]
		public LootTableOptions lootTable = new LootTableOptions();
		[NBT("item")]
		public ItemStack item = null;

		public override GameVersion AddedInVersion => GameVersion.Release_1(19, 4);

		public TileEntitySuspiciousBlock(string id) : base(id)
		{
			
		}

		public TileEntitySuspiciousBlock(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

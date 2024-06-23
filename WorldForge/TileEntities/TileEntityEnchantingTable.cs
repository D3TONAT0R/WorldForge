using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEnchantingTable : TileEntity
	{
		[NBT("CustomName")]
		public JSONTextComponent customName = null;

		public TileEntityEnchantingTable() : base("enchanting_table")
		{

		}

		public TileEntityEnchantingTable(NBTCompound nbt, out BlockCoord blockPos) : base(nbt, out blockPos)
		{

		}

		protected override string ResolveEntityID(GameVersion version)
		{
			return version >= GameVersion.Release_1(11) ? id : "EnchantTable";
		}
	}
}

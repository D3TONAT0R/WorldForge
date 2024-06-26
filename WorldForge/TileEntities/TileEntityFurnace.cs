using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityFurnace : TileEntityContainer
	{
		public ItemStack FurnaceInputSlot
		{
			get => items[0];
			set => items[0] = value;
		}

		public ItemStack FurnaceFuelSlot
		{
			get => items[1];
			set => items[1] = value;
		}

		public ItemStack FurnaceOutputSlot
		{
			get => items[2];
			set => items[2] = value;
		}

		[NBT("BurnTime")]
		public short burnTime = 0;
		[NBT("CookTime")]
		public short cookTime = 0;
		[NBT("CookTimeTotal")]
		public short cookTimeTotal = 200;

		[NBT("RecipesUsed")]
		public NBTCompound recipesUsed = null;

		public TileEntityFurnace(string id) : base(id, 3)
		{
			if(id.EndsWith("blast_furnace") || id.EndsWith("smoker"))
			{
				cookTimeTotal = 100;
			}
		}

		public TileEntityFurnace() : this("furnace")
		{

		}

		public TileEntityFurnace(NBTCompound compound, out BlockCoord blockPos) : base(compound, 3, out blockPos)
		{

		}

		protected override string ResolveTileEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				return "Furnace";
			}
		}
	}
}

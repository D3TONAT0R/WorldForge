using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
    public class TileEntityBrewingStand : TileEntityContainer
	{
		public ItemStack LeftPotionSlot
		{
			get => items[0];
			set => SetItem(0, value);
		}

		public ItemStack MiddlePotionSlot
		{
			get => items[1];
			set => SetItem(1, value);
		}

		public ItemStack RightPotionSlot
		{
			get => items[2];
			set => SetItem(2, value);
		}

		public ItemStack IngredientSlot
		{
			get => items[3];
			set => SetItem(3, value);
		}

		//Added in 1.9
		public ItemStack FuelSlot
		{
			get => items[4];
			set => SetItem(4, value);
		}

		//20 = Full
		[NBT("Fuel", "1.9")]
		public byte fuelLevel;

		[NBT("BrewTime")]
		public short brewTime;

		public TileEntityBrewingStand() : base("brewing_stand", 5)
		{
		}

		public TileEntityBrewingStand(NBTCompound nbt, out BlockCoord blockPos) : base(nbt, 5, out blockPos)
		{

		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{

		}

		protected override string ResolveEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				return "Cauldron";
			}
		}
	}
}

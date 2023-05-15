using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
{
	public class TileEntityFurnace : TileEntityContainer
	{
		public ItemStack FurnaceInputSlot
		{
			get => items[0];
			set => SetItem(0, value);
		}

		public ItemStack FurnaceFuelSlot
		{
			get => items[1];
			set => SetItem(1, value);
		}

		public ItemStack FurnaceOutputSlot
		{
			get => items[2];
			set => SetItem(2, value);
		}

		public TileEntityFurnace(string id, BlockCoord blockPos) : base(id, blockPos, 3)
		{

		}

		public TileEntityFurnace(NBTCompound compound) : base(compound, 3)
		{

		}
	}
}

using System;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityLectern : TileEntity
	{
		public ItemStack book = null;
		public int page = 0;

		public TileEntityLectern() : base("lectern")
		{
		}

		public TileEntityLectern(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
			if(compound.TryGet("Book", out NBTCompound bookNBT))
			{
				book = new ItemStack(bookNBT, out _);
				compound.TryGet("Page", out page);
			}
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(book != null)
			{
				nbt.Add("Book", book.ToNBT(null, version));
				nbt.Add("Page", page);
			}
		}
	}
}

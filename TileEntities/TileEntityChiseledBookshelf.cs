using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityChiseledBookshelf : TileEntity
	{
		public TileEntityChiseledBookshelf() : base("chiseled_bookshelf")
		{
		}

		public TileEntityChiseledBookshelf(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}
	}
}

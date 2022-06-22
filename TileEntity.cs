using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class TileEntity
	{

		public CompoundContainer NBTCompound
		{
			get;
			private set;
		}

		public int BlockPosX
		{
			get => NBTCompound.Get<int>("x");
			private set => NBTCompound.Set("x", value);
		}
		public int BlockPosY
		{
			get => NBTCompound.Get<int>("y");
			private set => NBTCompound.Set("y", value);
		}
		public int BlockPosZ
		{
			get => NBTCompound.Get<int>("z");
			private set => NBTCompound.Set("z", value);
		}

		public TileEntity(int x, int y, int z)
		{
			NBTCompound = new CompoundContainer();
			NBTCompound.Add("x", x);
			NBTCompound.Add("y", y);
			NBTCompound.Add("z", z);
		}

		public TileEntity(CompoundContainer compound)
		{
			NBTCompound = compound;
		}
	}
}

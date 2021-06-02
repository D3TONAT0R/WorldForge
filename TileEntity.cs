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
			get;
			private set;
		}
		public int BlockPosY
		{
			get;
			private set;
		}
		public int BlockPosZ
		{
			get;
			private set;
		}

		public TileEntity(CompoundContainer compound)
		{
			NBTCompound = compound;
			BlockPosX = compound.Get<int>("x");
			BlockPosY = compound.Get<int>("y");
			BlockPosZ = compound.Get<int>("z");
		}
	}
}

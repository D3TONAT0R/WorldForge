using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class Entity
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

		public Entity(CompoundContainer compound)
		{
			NBTCompound = compound;
			ListContainer pos = compound.GetAsList("Pos");
			BlockPosX = (int)Math.Floor(pos.Get<double>(0));
			BlockPosY = (int)Math.Floor(pos.Get<double>(1));
			BlockPosZ = (int)Math.Floor(pos.Get<double>(2));
		}
	}
}

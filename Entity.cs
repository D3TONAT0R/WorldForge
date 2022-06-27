using MCUtils.NBT;
using System;

namespace MCUtils
{
	public class Entity
	{

		public NBTCompound NBTCompound
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

		public Entity(NBTCompound compound)
		{
			NBTCompound = compound;
			NBTList pos = compound.GetAsList("Pos");
			BlockPosX = (int)Math.Floor(pos.Get<double>(0));
			BlockPosY = (int)Math.Floor(pos.Get<double>(1));
			BlockPosZ = (int)Math.Floor(pos.Get<double>(2));
		}
	}
}

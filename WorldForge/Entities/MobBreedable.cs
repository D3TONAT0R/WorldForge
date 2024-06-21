using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBreedable : Mob
	{
		[NBT("Age")]
		public int age = 0;
		[NBT("ForcedAge")]
		public int forcedAge = 0;
		[NBT("InLove")]
		public int inLove = 0;
		[NBT("LoveCause")]
		public UUID loveCause = null;

		public MobBreedable(NBTCompound compound) : base(compound)
		{

		}

		public MobBreedable(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

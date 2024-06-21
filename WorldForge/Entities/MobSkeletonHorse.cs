using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSkeletonHorse : MobHorseBase
	{
		[NBT("SkeletonTrap")]
		public bool skeletonTrap = false;
		[NBT("SkeletonTrapTime")]
		public int skeletonTrapTime = 0;

		public MobSkeletonHorse(NBTCompound compound) : base(compound)
		{

		}

		public MobSkeletonHorse(Vector3 position) : base("minecraft:skeleton_horse", position)
		{

		}
	}
}

﻿using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombiePigman : MobZombieBase
	{
		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobZombiePigman(NBTCompound compound) : base(compound)
		{

		}

		public MobZombiePigman(Vector3 position) : base("minecraft:zombie_pigman", position)
		{

		}
	}
}

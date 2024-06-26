﻿using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombieVillager : MobVillagerBase
	{
		[NBT("CanBreakDoors")]
		public bool canBreakDoors = false;
		[NBT("IsBaby")]
		public bool isBaby = false;

		[NBT("DrownedConversionTime", "1.13")]
		public int drownedConversionTime = -1;
		[NBT("InWaterTime", "1.13")]
		public int inWaterTime = -1;

		[NBT("ConversionTime")]
		public int conversionTime = -1;
		[NBT("ConversionPlayer")]
		public UUID conversionPlayer = null;

		public MobZombieVillager(NBTCompound compound) : base(compound)
		{

		}

		public MobZombieVillager(Vector3 position) : base("minecraft:zombie_villager", position)
		{

		}
	}
}

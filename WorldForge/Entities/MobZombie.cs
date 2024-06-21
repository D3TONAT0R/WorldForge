using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombie : Mob
	{
		[NBT("CanBreakDoors")]
		public bool canBreakDoors = false;
		[NBT("IsBaby")]
		public bool isBaby = false;

		[NBT("DrownedConversionTime", "1.13")]
		public int drownedConversionTime = -1;
		[NBT("InWaterTime", "1.13")]
		public int inWaterTime = -1;

		public MobZombie(NBTCompound compound) : base(compound)
		{

		}

		public MobZombie(Vector3 position) : base("minecraft:zombie", position)
		{

		}
	}
}

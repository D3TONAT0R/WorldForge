using WorldForge.NBT;

namespace WorldForge.Entities
{
	public abstract class MobZombieBase : Mob
	{
		[NBT("CanBreakDoors")]
		public bool canBreakDoors = false;
		[NBT("IsBaby")]
		public bool isBaby = false;

		[NBT("DrownedConversionTime", "1.13")]
		public int drownedConversionTime = -1;
		[NBT("InWaterTime", "1.13")]
		public int inWaterTime = -1;

		public MobZombieBase(NBTCompound compound) : base(compound)
		{

		}

		public MobZombieBase(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

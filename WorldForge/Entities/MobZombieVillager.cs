using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombieVillager : MobZombie
	{
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

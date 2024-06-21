using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombieHorse : MobHorseBase
	{
		public MobZombieHorse(NBTCompound compound) : base(compound)
		{

		}

		public MobZombieHorse(Vector3 position) : base("minecraft:zombie_horse", position)
		{

		}
	}
}

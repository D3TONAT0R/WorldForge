using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombie : MobZombieBase
	{
		public MobZombie(NBTCompound compound) : base(compound)
		{

		}

		public MobZombie(Vector3 position) : base("minecraft:zombie", position)
		{

		}
	}
}

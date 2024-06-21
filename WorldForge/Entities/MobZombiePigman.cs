using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZombiePigman : Mob
	{
		public MobZombiePigman(NBTCompound compound) : base(compound)
		{

		}

		public MobZombiePigman(Vector3 position) : base("minecraft:zombie_pigman", position)
		{

		}
	}
}

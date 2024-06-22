using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobElderGuardian : Mob
	{
		public MobElderGuardian(NBTCompound compound) : base(compound)
		{

		}

		public MobElderGuardian(Vector3 position) : base("minecraft:elder_guardian", position)
		{

		}
	}
}

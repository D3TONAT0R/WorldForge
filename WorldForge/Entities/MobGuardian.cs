using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobGuardian : Mob
	{
		public MobGuardian(NBTCompound compound) : base(compound)
		{

		}

		public MobGuardian(Vector3 position) : base("minecraft:guardian", position)
		{

		}
	}
}

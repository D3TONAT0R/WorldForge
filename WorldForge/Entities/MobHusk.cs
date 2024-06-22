using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobHusk : MobZombieBase
	{
		public MobHusk(NBTCompound compound) : base(compound)
		{

		}

		public MobHusk(Vector3 position) : base("minecraft:husk", position)
		{

		}
	}
}

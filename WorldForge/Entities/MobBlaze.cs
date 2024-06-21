using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBlaze : Mob
	{

		public MobBlaze(NBTCompound compound) : base(compound)
		{

		}

		public MobBlaze(Vector3 position) : base("minecraft:blaze", position)
		{

		}
	}
}

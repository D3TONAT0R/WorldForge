using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobStray : Mob
	{
		public MobStray(NBTCompound compound) : base(compound)
		{

		}

		public MobStray(Vector3 position) : base("minecraft:stray", position)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobGiant : Mob
	{

		public MobGiant(NBTCompound compound) : base(compound)
		{

		}

		public MobGiant(Vector3 position) : base("minecraft:giant", position)
		{

		}
	}
}

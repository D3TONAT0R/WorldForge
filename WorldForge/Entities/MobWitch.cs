using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWitch : MobRaidSpawnable
	{
		public MobWitch(NBTCompound compound) : base(compound)
		{

		}

		public MobWitch(Vector3 position) : base("minecraft:witch", position)
		{

		}
	}
}

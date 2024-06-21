using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSquid : Mob
	{
		public MobSquid(NBTCompound compound) : base(compound)
		{

		}

		public MobSquid(Vector3 position) : base("minecraft:squid", position)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSpider : Mob
	{
		public MobSpider(NBTCompound compound) : base(compound)
		{

		}

		public MobSpider(Vector3 position) : base("minecraft:spider", position)
		{

		}
	}
}

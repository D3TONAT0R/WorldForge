using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCaveSpider : Mob
	{
		public MobCaveSpider(NBTCompound compound) : base(compound)
		{

		}

		public MobCaveSpider(Vector3 position) : base("minecraft:cave_spider", position)
		{

		}
	}
}

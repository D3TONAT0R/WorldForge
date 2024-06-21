using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSkeleton : Mob
	{
		[NBT("StrayConversionTime", "1.17")]
		public int strayConversionTime;

		public MobSkeleton(NBTCompound compound) : base(compound)
		{

		}

		public MobSkeleton(Vector3 position) : base("minecraft:skeleton", position)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWitherSkeleton : Mob
	{
		public MobWitherSkeleton(NBTCompound compound) : base(compound)
		{

		}

		public MobWitherSkeleton(Vector3 position) : base("minecraft:wither_skeleton", position)
		{

		}
	}
}

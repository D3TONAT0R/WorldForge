using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBat : Mob
	{
		[NBT("BatFlags")]
		public bool hanging = false;

		public MobBat(NBTCompound compound) : base(compound)
		{

		}

		public MobBat(Vector3 position) : base("minecraft:bat", position)
		{

		}
	}
}

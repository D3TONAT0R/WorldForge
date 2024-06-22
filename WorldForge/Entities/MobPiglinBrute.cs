using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPiglinBrute : Mob
	{
		[NBT("IsImmuneToZombification")]
		public bool isImmuneToZombification = false;
		[NBT("TimeInOverworld")]
		public int timeInOverworld = 0;

		public MobPiglinBrute(NBTCompound compound) : base(compound)
		{

		}

		public MobPiglinBrute(Vector3 position) : base("minecraft:piglin_brute", position)
		{

		}
	}
}

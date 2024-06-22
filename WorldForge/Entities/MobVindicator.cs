using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobVindicator : MobRaidSpawnable
	{
		[NBT("Johnny")]
		public bool johnny = false;

		public MobVindicator(NBTCompound compound) : base(compound)
		{

		}

		public MobVindicator(Vector3 position) : base("minecraft:vindicator", position)
		{

		}
	}
}

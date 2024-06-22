using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobEndermite : Mob
	{
		[NBT("Lifetime")]
		public int lifetime = 0;

		public MobEndermite(NBTCompound compound) : base(compound)
		{

		}

		public MobEndermite(Vector3 position) : base("minecraft:endermite", position)
		{

		}
	}
}

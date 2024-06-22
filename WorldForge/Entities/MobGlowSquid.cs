using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobGlowSquid : Mob
	{
		[NBT("DarkTicksRemaining")]
		public int darkTicksRemaining = 0;

		public MobGlowSquid(NBTCompound compound) : base(compound)
		{

		}

		public MobGlowSquid(Vector3 position) : base("minecraft:glow_squid", position)
		{

		}
	}
}

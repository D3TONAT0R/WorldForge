using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWither : Mob
	{
		[NBT("Invul")]
		public int invulnerabilityTicks = 0;

		public MobWither(NBTCompound compound) : base(compound)
		{

		}

		public MobWither(Vector3 position) : base("minecraft:wither", position)
		{

		}
	}
}

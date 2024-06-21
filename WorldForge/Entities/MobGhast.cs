using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobGhast : Mob
	{
		[NBT("ExplosionPower")]
		public int explosionPower = 1;

		public MobGhast(NBTCompound compound) : base(compound)
		{

		}

		public MobGhast(Vector3 position) : base("minecraft:ghast", position)
		{

		}
	}
}

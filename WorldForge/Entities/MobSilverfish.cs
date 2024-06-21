using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSilverfish : Mob
	{
		public MobSilverfish(NBTCompound compound) : base(compound)
		{

		}

		public MobSilverfish(Vector3 position) : base("minecraft:silverfish", position)
		{

		}
	}
}

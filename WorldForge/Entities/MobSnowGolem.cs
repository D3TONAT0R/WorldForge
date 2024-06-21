using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSnowGolem : Mob
	{
		[NBT("Pumpkin")]
		public bool pumpkin = true;

		public MobSnowGolem(NBTCompound compound) : base(compound)
		{

		}

		public MobSnowGolem(Vector3 position) : base("minecraft:snow_golem", position)
		{

		}
	}
}

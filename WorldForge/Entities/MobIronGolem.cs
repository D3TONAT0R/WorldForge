using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobIronGolem : MobBreedable
	{
		[NBT("PlayerCreated")]
		public bool playerCreated = false;

		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobIronGolem(NBTCompound compound) : base(compound)
		{

		}

		public MobIronGolem(Vector3 position) : base("minecraft:iron_golem", position)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPolarBear : MobBreedable
	{
		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobPolarBear(NBTCompound compound) : base(compound)
		{

		}

		public MobPolarBear(Vector3 position) : base("minecraft:polar_bear", position)
		{

		}
	}
}

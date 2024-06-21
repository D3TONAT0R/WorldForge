using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobHorse : MobHorseBase
	{
		[NBT("Variant")]
		public int variant;

		public MobHorse(NBTCompound compound) : base(compound)
		{

		}

		public MobHorse(Vector3 position) : base("minecraft:horse", position)
		{

		}
	}
}

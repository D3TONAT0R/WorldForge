using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPufferfish : MobFishBase
	{
		[NBT("PuffState")]
		public int puffState = 0;

		public MobPufferfish(NBTCompound compound) : base(compound)
		{

		}

		public MobPufferfish(Vector3 position) : base("minecraft:pufferfish", position)
		{

		}
	}
}

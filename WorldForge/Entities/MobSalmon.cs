using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSalmon : MobFishBase
	{
		public MobSalmon(NBTCompound compound) : base(compound)
		{

		}

		public MobSalmon(Vector3 position) : base("minecraft:salmon", position)
		{

		}
	}
}

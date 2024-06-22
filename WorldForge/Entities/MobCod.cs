using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCod : MobFishBase
	{
		public MobCod(NBTCompound compound) : base(compound)
		{

		}

		public MobCod(Vector3 position) : base("minecraft:cod", position)
		{

		}
	}
}

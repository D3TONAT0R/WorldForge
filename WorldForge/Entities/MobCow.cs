using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCow : MobBreedable
	{
		public MobCow(NBTCompound compound) : base(compound)
		{

		}

		public MobCow(Vector3 position) : base("minecraft:cow", position)
		{

		}
	}
}

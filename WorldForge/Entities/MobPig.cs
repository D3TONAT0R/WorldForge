using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPig : MobBreedable
	{
		[NBT("Saddle")]
		public bool saddle = false;

		public MobPig(NBTCompound compound) : base(compound)
		{

		}

		public MobPig(Vector3 position) : base("minecraft:pig", position)
		{

		}
	}
}

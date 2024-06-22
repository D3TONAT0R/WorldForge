using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobStrider : MobBreedable
	{
		[NBT("Saddle")]
		public bool saddle = false;

		public MobStrider(NBTCompound compound) : base(compound)
		{

		}

		public MobStrider(Vector3 position) : base("minecraft:strider", position)
		{

		}
	}
}

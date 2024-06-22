using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobMagmaCube : MobSlime
	{
		public MobMagmaCube(NBTCompound compound) : base(compound)
		{

		}

		public MobMagmaCube(Vector3 position, int size) : base("minecraft:magma_cube", position, size)
		{

		}
	}
}

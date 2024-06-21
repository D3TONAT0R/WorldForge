using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobMagmaCube : MobSlime
	{
		protected override string ID => "minecraft:magma_cube";

		public MobMagmaCube(NBTCompound compound) : base(compound)
		{

		}

		public MobMagmaCube(Vector3 position, int size) : base(position, size)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobOcelot : MobBreedable
	{
		[NBT("Trusting")]
		public bool trusting = false;

		public MobOcelot(NBTCompound compound) : base(compound)
		{

		}

		public MobOcelot(Vector3 position) : base("minecraft:ocelot", position)
		{

		}
	}
}

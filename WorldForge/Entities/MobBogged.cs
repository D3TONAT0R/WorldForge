using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBogged : MobBreedable
	{
		public MobBogged(NBTCompound compound) : base(compound)
		{
			
		}

		public MobBogged(Vector3 position) : base("minecraft:bogged", position)
		{
			
		}
	}
}

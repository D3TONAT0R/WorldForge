using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobTadpole : MobFishBase
	{
		[NBT("Age")]
		public int age = 0;

		public MobTadpole(NBTCompound compound) : base(compound)
		{
			
		}

		public MobTadpole(Vector3 position) : base("minecraft:tadpole", position)
		{
			
		}
	}
}

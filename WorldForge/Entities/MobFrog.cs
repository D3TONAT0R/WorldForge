using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobFrog : MobBreedable
	{
		[NBT("variant")]
		public string variant = "minecraft:temperate";

		public MobFrog(NBTCompound compound) : base(compound)
		{
			
		}

		public MobFrog(Vector3 position) : base("minecraft:frog", position)
		{
			
		}
	}
}

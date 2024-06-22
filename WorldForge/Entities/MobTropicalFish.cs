using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobTropicalFish : MobFishBase
	{
		[NBT("Variant")]
		public int variant = 0;

		public MobTropicalFish(NBTCompound compound) : base(compound)
		{

		}

		public MobTropicalFish(Vector3 position) : base("minecraft:tropical_fish", position)
		{

		}
	}
}

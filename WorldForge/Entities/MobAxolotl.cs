using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobAxolotl : MobBreedable
	{
		public enum Variant : int
		{
			Lucy = 0,
			Wild = 1,
			Gold = 2,
			Cyan = 3,
			Blue = 4
		}

		[NBT("FromBucket")]
		public bool fromBucket = false;
		[NBT("Variant")]
		public Variant variant = Variant.Lucy;

		public MobAxolotl(NBTCompound compound) : base(compound)
		{

		}

		public MobAxolotl(Vector3 position) : base("minecraft:axolotl", position)
		{

		}
	}
}

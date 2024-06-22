using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobGoat : MobBreedable
	{
		[NBT("HasLeftHorn")]
		public bool hasLeftHorn = true;
		[NBT("HasRightHorn")]
		public bool hasRightHorn = true;
		[NBT("IsScreamingGoat")]
		public bool isScreamingGoat = false;

		public MobGoat(NBTCompound compound) : base(compound)
		{

		}

		public MobGoat(Vector3 position) : base("minecraft:goat", position)
		{

		}
	}
}

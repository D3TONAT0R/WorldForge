using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobFishBase : Mob
	{
		[NBT("FromBucket")]
		public bool fromBucket = false;

		public MobFishBase(NBTCompound compound) : base(compound)
		{

		}

		public MobFishBase(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

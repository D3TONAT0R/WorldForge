using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobParrot : Mob
	{
		public enum Variant : int
		{
			Red = 0,
			Blue = 1,
			Green = 2,
			Cyan = 3,
			Gray = 4
		}

		[NBT("Variant")]
		public Variant variant;

		//Tameable data
		[NBT("Owner")]
		public UUID owner = null;
		[NBT("Sitting")]
		public bool sitting = false;

		public MobParrot(NBTCompound compound) : base(compound)
		{

		}

		public MobParrot(Vector3 position) : base("minecraft:parrot", position)
		{

		}
	}
}

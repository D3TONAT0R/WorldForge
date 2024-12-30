using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCat : MobBreedable
	{
		[NBT("CollarColor")]
		public ColorType collarColor = ColorType.Red;
		[NBT("variant")]
		public string variant = "minecraft:white";

		//Tameable data
		[NBT("Owner")]
		public UUID owner = null;
		[NBT("Sitting")]
		public bool sitting = false;

		public MobCat(NBTCompound compound) : base(compound)
		{

		}

		public MobCat(Vector3 position) : base("minecraft:cat", position)
		{

		}
	}
}

using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWolf : MobBreedable
	{
		[NBT("CollarColor")]
		public ColorType collarColor = ColorType.Red;
		[NBT("variant")]
		public string variant = "minecraft:pale";

		//Tameable data
		[NBT("Owner")]
		public UUID owner = null;
		[NBT("Sitting")]
		public bool sitting = false;

		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobWolf(NBTCompound compound) : base(compound)
		{

		}

		public MobWolf(Vector3 position) : base("minecraft:wolf", position)
		{

		}
	}
}

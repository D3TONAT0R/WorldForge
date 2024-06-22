using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobArmadillo : MobBreedable
	{
		public const int SCUTE_INTERVAL_MIN = 6000;
		public const int SCUTE_INTERVAL_MAX = 12000;

		[NBT("scute_time")]
		public int scuteTime = (SCUTE_INTERVAL_MIN + SCUTE_INTERVAL_MAX) / 2;
		[NBT("state")]
		public string state = "idle";

		public MobArmadillo(NBTCompound compound) : base(compound)
		{
			
		}

		public MobArmadillo(Vector3 position) : base("minecraft:armadillo", position)
		{
			
		}
	}
}

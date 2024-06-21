using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobChicken : MobBreedable
	{
		public const int EGG_LAY_INTERVAL_MIN = 6000;
		public const int EGG_LAY_INTERVAL_MAX = 12000;

		[NBT("EggLayTime")]
		public int eggLayTime = (EGG_LAY_INTERVAL_MIN + EGG_LAY_INTERVAL_MAX) / 2;
		[NBT("IsChickenJockey")]
		public bool isChickenJockey = false;

		public MobChicken(NBTCompound compound) : base(compound)
		{

		}

		public MobChicken(Vector3 position) : base("minecraft:chicken", position)
		{

		}
	}
}

using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobHorseBase : MobBreedable
	{
		[NBT("Bred")]
		public bool bred = false;
		[NBT("EatingHaystack")]
		public bool eatingHaystack = false;
		[NBT("Owner")]
		public UUID owner = null;
		[NBT("SaddleItem")]
		public ItemStack saddle = null;
		[NBT("Tame")]
		public bool tame = false;
		[NBT("Temper")]
		public int temper = 0;

		public MobHorseBase(NBTCompound compound) : base(compound)
		{

		}

		public MobHorseBase(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

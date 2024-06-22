using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobLlama : MobBreedable
	{
		public enum Variant : int
		{
			Creamy = 0,
			White = 1,
			Brown = 2,
			Gray = 3
		}

		[NBT("Bred")]
		public bool bred = false;
		[NBT("ChestedHorse")]
		public bool hasChests = false;
		[NBT("EatingHaystack")]
		public bool eatingHaystack = false;
		[NBT("Items")]
		public List<ItemStack> items = null;
		[NBT("Owner")]
		public UUID owner = null;
		[NBT("Variant")]
		public Variant variant = Variant.Creamy;
		[NBT("Strength")]
		public int strength = 3;
		[NBT("Tame")]
		public bool tame = false;
		[NBT("Temper")]
		public int temper = 0;

		public MobLlama(NBTCompound compound) : base(compound)
		{

		}

		public MobLlama(Vector3 position) : base("minecraft:llama", position)
		{

		}

		protected MobLlama(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

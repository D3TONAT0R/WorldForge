using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobDonkey : MobHorseBase
	{
		[NBT("ChestedHorse")]
		public bool chested = false;
		[NBT("Items")]
		public List<ItemStack> items = null;

		public MobDonkey(NBTCompound compound) : base(compound)
		{

		}

		public MobDonkey(Vector3 position) : base("minecraft:donkey", position)
		{

		}
	}
}

using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobMule : MobHorseBase
	{
		[NBT("ChestedHorse")]
		public bool chested = false;
		[NBT("Items")]
		public List<ItemStack> items = null;

		public MobMule(NBTCompound compound) : base(compound)
		{

		}

		public MobMule(Vector3 position) : base("minecraft:mule", position)
		{

		}
	}
}

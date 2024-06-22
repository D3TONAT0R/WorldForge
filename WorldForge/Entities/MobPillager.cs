using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPillager : MobRaidSpawnable
	{
		[NBT("Inventory")]
		public List<ItemStack> inventory = new List<ItemStack>();

		public MobPillager(NBTCompound compound) : base(compound)
		{

		}

		public MobPillager(Vector3 position) : base("minecraft:pillager", position)
		{

		}
	}
}

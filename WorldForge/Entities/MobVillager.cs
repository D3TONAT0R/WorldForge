using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobVillager : MobBreedable
	{
		[NBT("Inventory")]
		public List<ItemStack> inventory = new List<ItemStack>();
		[NBT("LastRestock")]
		public long lastRestock = 0;
		[NBT("RestocksToday")]
		public int restocksToday = 0;
		[NBT("LastGossipDecay")]
		public long lastGossipDecay = 0;
		[NBT("Willing")]
		public bool willing = false;

		public MobVillager(NBTCompound compound) : base(compound)
		{

		}

		public MobVillager(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

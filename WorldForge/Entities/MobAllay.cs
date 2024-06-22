using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobAllay : MobFishBase
	{
		[NBT("CanDuplicate")]
		public bool canDuplicate = true;
		[NBT("DuplicationCooldown")]
		public long duplicationCooldown = 0;
		[NBT("Inventory")]
		public List<ItemStack> inventory = new List<ItemStack>();
		[NBT("listener")]
		public NBTCompound eventListener = null;

		public MobAllay(NBTCompound compound) : base(compound)
		{
			
		}

		public MobAllay(Vector3 position) : base("minecraft:allay", position)
		{
			
		}
	}
}

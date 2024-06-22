using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPiglin : Mob
	{
		[NBT("CannotHunt")]
		public bool cannotHunt = false;
		[NBT("IsBaby")]
		public bool isBaby = false;
		[NBT("Inventory")]
		public List<ItemStack> items = new List<ItemStack>();
		[NBT("IsImmuneToZombification")]
		public bool isImmuneToZombification = false;
		[NBT("TimeInOverworld")]
		public int timeInOverworld = 0;

		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobPiglin(NBTCompound compound) : base(compound)
		{

		}

		public MobPiglin(Vector3 position) : base("minecraft:piglin", position)
		{

		}
	}
}

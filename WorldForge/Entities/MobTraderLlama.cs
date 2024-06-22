using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobTraderLlama : MobLlama
	{
	
		public MobTraderLlama(NBTCompound compound) : base(compound)
		{

		}

		public MobTraderLlama(Vector3 position) : base("minecraft:trader_llama", position)
		{

		}
	}
}

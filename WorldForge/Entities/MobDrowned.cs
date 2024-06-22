using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobDrowned : MobZombieBase
	{
		[NBT("Gossips")]
		public List<NBTCompound> gossips = new List<NBTCompound>();

		public MobDrowned(NBTCompound compound) : base(compound)
		{

		}

		public MobDrowned(Vector3 position) : base("minecraft:drowned", position)
		{

		}
	}
}

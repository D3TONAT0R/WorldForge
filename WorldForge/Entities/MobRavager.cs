using System.Collections.Generic;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobRavager : MobRaidSpawnable
	{
		[NBT("AttackTick")]
		public int attackCooldownTicks = 0;
		[NBT("RoarTick")]
		public int roarCooldownTicks = 0;
		[NBT("StunTick")]
		public int stunCooldownTicks = 0;

		public MobRavager(NBTCompound compound) : base(compound)
		{

		}

		public MobRavager(Vector3 position) : base("minecraft:ravager", position)
		{

		}
	}
}

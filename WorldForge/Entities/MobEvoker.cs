using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobEvoker : MobRaidSpawnable
	{
		[NBT("SpellTicks")]
		public int spellTicks = 0;

		public MobEvoker(NBTCompound compound) : base(compound)
		{

		}

		public MobEvoker(Vector3 position) : base("minecraft:evoker", position)
		{

		}
	}
}

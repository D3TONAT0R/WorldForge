using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobIllusioner : MobRaidSpawnable
	{
		[NBT("SpellTicks")]
		public int spellTicks = 0;

		public MobIllusioner(NBTCompound compound) : base(compound)
		{

		}

		public MobIllusioner(Vector3 position) : base("minecraft:illusioner", position)
		{

		}
	}
}

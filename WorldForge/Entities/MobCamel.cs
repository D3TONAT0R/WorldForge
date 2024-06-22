using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCamel : MobHorseBase
	{
		[NBT("LastPoseTick")]
		public long lastPoseTick = 0;

		public MobCamel(NBTCompound compound) : base(compound)
		{
			
		}

		public MobCamel(Vector3 position) : base("minecraft:camel", position)
		{
			
		}
	}
}

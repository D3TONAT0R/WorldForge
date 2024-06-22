using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSniffer : MobBreedable
	{
		public MobSniffer(NBTCompound compound) : base(compound)
		{
			
		}

		public MobSniffer(Vector3 position) : base("minecraft:sniffer", position)
		{
			
		}
	}
}

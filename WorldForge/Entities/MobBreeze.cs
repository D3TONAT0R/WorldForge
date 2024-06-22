using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBreeze : Mob
	{
		public MobBreeze(NBTCompound compound) : base(compound)
		{
			
		}

		public MobBreeze(Vector3 position) : base("minecraft:breeze", position)
		{
			
		}
	}
}

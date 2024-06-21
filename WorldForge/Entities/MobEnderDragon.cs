using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobEnderDragon : Mob
	{
		[NBT("DragonPhase")]
		public int dragonPhase = 0;

		public MobEnderDragon(NBTCompound compound) : base(compound)
		{

		}

		public MobEnderDragon(Vector3 position) : base("minecraft:ender_dragon", position)
		{

		}
	}
}

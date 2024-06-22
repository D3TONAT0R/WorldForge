using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobZoglin : Mob
	{
		[NBT("IsBaby")]
		public bool isBaby = false;

		public MobZoglin(NBTCompound compound) : base(compound)
		{

		}

		public MobZoglin(Vector3 position) : base("minecraft:zoglin", position)
		{

		}
	}
}

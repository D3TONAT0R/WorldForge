using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobHoglin : MobBreedable
	{
		[NBT("CannotBeHunted")]
		public bool cannotBeHunted = false;
		[NBT("IsImmuneToZombification")]
		public bool isImmuneToZombification = false;
		[NBT("TimeInOverworld")]
		public int timeInOverworld = 0;

		public MobHoglin(NBTCompound compound) : base(compound)
		{

		}

		public MobHoglin(Vector3 position) : base("minecraft:hoglin", position)
		{

		}
	}
}

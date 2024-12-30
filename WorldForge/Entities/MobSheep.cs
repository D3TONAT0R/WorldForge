using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSheep : MobBreedable
	{
		[NBT("Color")]
		public ColorType color = ColorType.White;
		[NBT("Sheared")]
		public bool sheared = false;

		public MobSheep(NBTCompound compound) : base(compound)
		{

		}

		public MobSheep(Vector3 position) : base("minecraft:spider", position)
		{

		}
	}
}

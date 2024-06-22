using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobRabbit : MobBreedable
	{
		[NBT("MoreCarrotTicks")]
		public int moreCarrotTicks = 0;
		[NBT("RabbitType")]
		public int rabbitType = 0;

		public MobRabbit(NBTCompound compound) : base(compound)
		{

		}

		public MobRabbit(Vector3 position) : base("minecraft:rabbit", position)
		{

		}
	}
}

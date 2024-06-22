using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobVex : Mob
	{
		//BoundX, BoundY, BoundZ
		public BlockCoord? boundPos;
		[NBT("LifeTicks")]
		public int lifeTicks = 0;

		public MobVex(NBTCompound compound) : base(compound)
		{
			if(compound.Contains("BoundX"))
			{
				boundPos = new BlockCoord(compound.Get<int>("BoundX"), compound.Get<int>("BoundY"), compound.Get<int>("BoundZ"));
			}
		}

		public MobVex(Vector3 position, int lifeTicks) : base("minecraft:vex", position)
		{
			this.lifeTicks = lifeTicks;
		}
	}
}

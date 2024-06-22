using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobBee : MobBreedable
	{
		[NBT("CannotEnterHiveTicks")]
		public int cannotEnterHiveTicks = 0;
		[NBT("HasNectar")]
		public bool hasNectar = false;
		[NBT("HasStung")]
		public bool hasStung = false;
		[NBT("TicksSincePollination")]
		public int ticksSincePollination = 0;
		[NBT("CropsGrownSincePollination")]
		public int cropsGrownSincePollination = 0;
		[NBT("flower_pos")]
		public BlockCoord flowerPos = default;
		[NBT("HivePos")]
		public BlockCoord hivePos = default;

		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobBee(NBTCompound compound) : base(compound)
		{

		}

		public MobBee(Vector3 position) : base("minecraft:bee", position)
		{

		}
	}
}

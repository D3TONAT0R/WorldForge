using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobDolphin : MobFishBase
	{
		[NBT("CanFindTreasure")]
		public bool canFindTreasure = false;
		[NBT("GotFish")]
		public bool gotFish = false;
		//TreasurePos
		public BlockCoord treasurePos = default;

		public MobDolphin(NBTCompound compound) : base(compound)
		{
			treasurePos = new BlockCoord(compound.Get<int>("TreasurePosX"), compound.Get<int>("TreasurePosY"), compound.Get<int>("TreasurePosZ"));
		}

		public MobDolphin(Vector3 position) : base("minecraft:dolphin", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			comp.Add("TreasurePosX", treasurePos.x);
			comp.Add("TreasurePosY", treasurePos.y);
			comp.Add("TreasurePosZ", treasurePos.z);
			return comp;
		}
	}
}

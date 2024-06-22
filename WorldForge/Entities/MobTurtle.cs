using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobTurtle : Mob
	{
		[NBT("HasEgg")]
		public bool hasEgg = false;

		public BlockCoord homePosition = default;
		public BlockCoord? travelPosition = null;

		public MobTurtle(NBTCompound compound) : base(compound)
		{
			homePosition = new BlockCoord(compound.Get<int>("HomePosX"), compound.Get<int>("HomePosY"), compound.Get<int>("HomePosZ"));
			if(compound.Contains("TravelPosX"))
			{
				travelPosition = new BlockCoord(compound.Get<int>("TravelPosX"), compound.Get<int>("TravelPosY"), compound.Get<int>("TravelPosZ"));
			}
		}

		public MobTurtle(Vector3 position) : base("minecraft:turtle", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			comp.Add("HomePosX", homePosition.x);
			comp.Add("HomePosY", homePosition.y);
			comp.Add("HomePosZ", homePosition.z);
			if(travelPosition.HasValue)
			{
				comp.Add("TravelPosX", travelPosition.Value.x);
				comp.Add("TravelPosY", travelPosition.Value.y);
				comp.Add("TravelPosZ", travelPosition.Value.z);
			}
			return comp;
		}
	}
}

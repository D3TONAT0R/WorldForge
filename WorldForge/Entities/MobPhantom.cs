using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPhantom : Mob
	{
		public BlockCoord? circlingPosition = null;
		[NBT("Size")]
		public int size = 0;

		public MobPhantom(NBTCompound compound) : base(compound)
		{
			if(compound.Contains("AX"))
			{
				circlingPosition = new BlockCoord(compound.Get<int>("AX"), compound.Get<int>("AY"), compound.Get<int>("AZ"));
			}
		}

		public MobPhantom(Vector3 position) : base("minecraft:phantom", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			if(circlingPosition.HasValue)
			{
				comp.Add("AX", circlingPosition.Value.x);
				comp.Add("AY", circlingPosition.Value.y);
				comp.Add("AZ", circlingPosition.Value.z);
			}
			return comp;
		}
	}
}

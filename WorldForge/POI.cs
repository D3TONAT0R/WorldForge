using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge
{
	public class POI : INBTConverter
	{
		[NBT]
		public BlockCoord position;
		[NBT]
		public string type;
		[NBT("free_tickets")]
		public int freeTickets = 1;

		public POI(BlockCoord position, string type, int freeTickets = 1)
		{
			this.position = position;
			this.type = type;
			this.freeTickets = freeTickets;
		}

		public POI(NBTCompound comp)
		{
			FromNBT(comp);
		}

		public object ToNBT(GameVersion version)
		{
			return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
		}

		public void FromNBT(object nbtData)
		{
			NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
		}
	}
}

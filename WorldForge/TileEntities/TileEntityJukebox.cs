using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityJukebox : TileEntity
	{
		public ItemStack recordItem;

		[NBT("IsPlaying", "1.19.1")]
		public bool isPlaying;
		[NBT("RecordStartTick", "1.19.1")]
		public long recordStartTick;
		[NBT("TickCount", "1.19.1")]
		public long tickCount;

		public TileEntityJukebox() : base("jukebox")
		{

		}

		public TileEntityJukebox(NBTCompound nbt, out BlockCoord blockPos) : base(nbt, out blockPos)
		{
			if(nbt.TryGet("RecordItem", out NBTCompound itemNBT))
			{
				recordItem = new ItemStack(itemNBT, out _);
			}
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
		{
			if(!recordItem.IsNull)
			{
				nbt.Add("RecordItem", recordItem.ToNBT(null, version));
			}
		}

		protected override string ResolveEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				return "RecordPlayer";
			}
		}
	}
}

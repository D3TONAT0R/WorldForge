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

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(recordItem != null && !recordItem.IsNull && recordItem.ToNBT(null, version, out var itemNbt))
			{
				nbt.Add("RecordItem", itemNbt);
			}
		}

		public override string ResolveTileEntityID(GameVersion version)
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

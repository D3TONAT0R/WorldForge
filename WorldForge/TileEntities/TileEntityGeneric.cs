using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityGeneric : TileEntity
	{
		public TileEntityGeneric(string id) : base(id)
		{

		}

		public TileEntityGeneric(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{

		}

		protected override string ResolveEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				switch(id)
				{
					case "flower_pot": return "FlowerPot";
					case "note_block": return "Music";
				}
				return id;
			}
		}
	}
}

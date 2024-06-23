using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityStructureBlock : TileEntity
	{
		[NBT("author")]
		public string author = "?";
		[NBT("ignoreEntities")]
		public bool ignoreEntities = false;
		[NBT("integrity")]
		public float integrity = 1f;
		[NBT("metadata")]
		public string metadata = "";
		[NBT("mirror")]
		public string mirror = "NONE";
		[NBT("mode")]
		public string mode = "SAVE";
		[NBT("posX")]
		public int posX = 0;
		[NBT("posY")]
		public int posY = 0;
		[NBT("posZ")]
		public int posZ = 0;
		[NBT("powered")]
		public bool powered = false;
		[NBT("rotation")]
		public string rotation = "NONE";
		[NBT("seed")]
		public long seed = 0;
		[NBT("showboundingbox")]
		public bool showBoundingBox = false;
		[NBT("sizeX")]
		public int sizeX = 0;
		[NBT("sizeY")]
		public int sizeY = 0;
		[NBT("sizeZ")]
		public int sizeZ = 0;

		public TileEntityStructureBlock() : base("structure_block")
		{

		}

		public TileEntityStructureBlock(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			throw new NotImplementedException();
		}

		protected override string ResolveEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				return "Structure";
			}
		}
	}
}

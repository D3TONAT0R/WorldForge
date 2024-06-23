using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityCommandBlock : TileEntity
	{
		[NBT("auto")]
		public bool auto = false;
		[NBT("Command")]
		public string command = "";
		[NBT("conditionMet")]
		public bool conditionMet = true;
		[NBT("LastExecution")]
		public long lastExecution = 0;
		[NBT("powered")]
		public bool powered = false;
		[NBT("SuccessCount")]
		public int successCount = 0;
		[NBT("TrackOutput")]
		public bool trackOutput = true;
		[NBT("UpdateLastExecution")]
		public bool updateLastExecution = true;

		[NBT("CustomName")]
		public JSONTextComponent customName = null;

		public TileEntityCommandBlock() : base("command_block")
		{

		}

		public TileEntityCommandBlock(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
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
				return "Control";
			}
		}
	}
}

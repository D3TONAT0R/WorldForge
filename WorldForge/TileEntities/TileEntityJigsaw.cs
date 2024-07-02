using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityJigsaw : TileEntity
	{
		[NBT("final_state")]
		public string finalState = "minecraft:air";
		[NBT("joint")]
		public string joint = "aligned";
		[NBT("name")]
		public string name = "";
		[NBT("pool")]
		public string pool = "";
		[NBT("target")]
		public string target = "";
		[NBT("selection_priority")]
		public int selectionPriority = 0;
		[NBT("placement_priority")]
		public int placementPriority = 0;

		public override GameVersion AddedInVersion => GameVersion.Release_1(14);

		public TileEntityJigsaw() : base("jigsaw")
		{

		}

		public TileEntityJigsaw(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

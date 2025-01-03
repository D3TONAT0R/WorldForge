using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityPiston : TileEntity
	{
		public enum Facing : int
		{
			Down = 0,
			Up = 1,
			North = 2,
			South = 3,
			West = 4,
			East = 5
		}

		[NBT("blockState")]
		public BlockState? blockState = null;
		[NBT("extending")]
		public bool extending = false;
		[NBT("facing")]
		public Facing facing = Facing.Up;
		[NBT("progress")]
		public float progress = 0.0f;
		[NBT("source")]
		public bool source = false;

		public TileEntityPiston() : base("piston")
		{

		}

		public TileEntityPiston(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			
		}
	}
}

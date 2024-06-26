using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEndGateway : TileEntity
	{
		public class ExitLocation : INBTConverter
		{
			[NBT("X")]
			public int x = 0;
			[NBT("Y")]
			public int y = 0;
			[NBT("Z")]
			public int z = 0;

			public ExitLocation()
			{

			}

			public ExitLocation(int x, int y, int z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		[NBT("Age")]
		public long age = 0;
		[NBT("ExactTeleport")]
		public bool exactTeleport = false;
		[NBT("ExitPortal")]
		public ExitLocation exitPortal = new ExitLocation();

		public TileEntityEndGateway() : base("end_gateway")
		{

		}

		public TileEntityEndGateway(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override string ResolveTileEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				return "EndGateway";
			}
		}
	}
}

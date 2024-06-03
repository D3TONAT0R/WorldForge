using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEndGateway : TileEntity
	{
		public TileEntityEndGateway() : base("end_gateway")
		{
		}

		public TileEntityEndGateway(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, GameVersion version)
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
				return "EndGateway";
			}
		}
	}
}

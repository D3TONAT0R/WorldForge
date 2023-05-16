using MCUtils.Coordinates;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntityEndGateway : TileEntity
	{
		public TileEntityEndGateway() : base("end_gateway")
		{
		}

		public TileEntityEndGateway(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new NotImplementedException();
		}

		protected override string ResolveEntityID(Version version)
		{
			if(version >= Version.Release_1(11))
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

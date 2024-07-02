﻿using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBed : TileEntity
	{

		public TileEntityBed() : base("bed")
		{

		}

		public TileEntityBed(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		public override string ResolveTileEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11, 0))
			{
				return "bed";
			}
			else
			{
				return "Bed";
			}
		}
	}
}

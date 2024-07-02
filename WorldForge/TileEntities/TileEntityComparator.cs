﻿using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityComparator : TileEntity
	{
		[NBT("OutputSignal")]
		public int outputSignal = 0;

		public TileEntityComparator() : base("comparator")
		{

		}

		public TileEntityComparator(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

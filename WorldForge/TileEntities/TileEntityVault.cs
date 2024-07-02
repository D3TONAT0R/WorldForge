using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityVault : TileEntity
	{
		[NBT("config")]
		public NBTCompound config;
		[NBT("server_data")]
		public NBTCompound serverData;
		[NBT("shared_data")]
		public NBTCompound sharedData;

		public TileEntityVault() : base("vault")
		{

		}

		public TileEntityVault(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

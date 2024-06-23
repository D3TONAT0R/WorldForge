﻿using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityEnderChest : TileEntity
	{
		public TileEntityEnderChest() : base("ender_chest")
		{

		}

		public TileEntityEnderChest(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override string ResolveEntityID(GameVersion version)
		{
			return version >= GameVersion.Release_1(11) ? id : "EnderChest";
		}
	}
}

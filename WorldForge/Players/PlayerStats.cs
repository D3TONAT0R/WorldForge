using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class PlayerStats
	{
		public PlayerData Parent { get; set; }

		public NBTCompound data;
	}
}

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

		public int? DataVersion { get; set; }

		public static PlayerStats FromJson(string json)
		{
			var stats = new PlayerStats();
			var nbt = NBTCompound.FromJson(json);
			stats.data = nbt.GetAsCompound("stats");
			if (stats.data.TryGet("DataVersion", out int dv)) stats.DataVersion = dv;
			return stats;
		}

		public static PlayerStats FromFile(string path)
		{
			return FromJson(System.IO.File.ReadAllText(path));
		}
	}
}

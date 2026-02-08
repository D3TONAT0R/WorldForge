using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class PlayerAdvancements
	{
		public PlayerData Parent { get; set; }

		public NBTCompound data;

		public int? DataVersion { get; set; }

		public static PlayerAdvancements FromJson(string json)
		{
			var advancements = new PlayerAdvancements();
			var nbt = NBTCompound.FromJson(json);
			if (nbt.TryTake("DataVersion", out int dv)) advancements.DataVersion = dv;
			advancements.data = nbt;
			return advancements;
		}

		public static PlayerAdvancements FromFile(string path)
		{
			return FromJson(System.IO.File.ReadAllText(path));
		}
	}
}

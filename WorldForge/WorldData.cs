using System;
using System.Collections.Generic;
using System.IO;
using WorldForge.Maps;
using WorldForge.NBT;

namespace WorldForge
{
	public class WorldData
	{
		public interface IData
		{
			void Save(string path, GameVersion version);
		}

		public class RaidsData : IData
		{
			[NBT("Raids")]
			public List<int> raidsList = new List<int>();
			[NBT("NextAvailableID")]
			public int nextAvailableId = 1;
			[NBT("Tick")]
			public int tick = 0;

			public static RaidsData Load(NBTFile file)
			{
				var data = new RaidsData();
				NBTConverter.LoadFromNBT(file.contents.GetAsCompound("data"), data);
				return data;
			}

			public NBTFile ToNBT(GameVersion version)
			{
				var file = new NBTFile();
				var comp = NBTConverter.WriteToNBT(this, new NBTCompound(), version);
				var dv = version.GetDataVersion();
				if(dv.HasValue) file.contents.Add("DataVersion", version.GetDataVersion());
				file.contents.Add("data", comp);
				return file;
			}

			public void Save(string path, GameVersion version)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var file = ToNBT(version);
				file.SaveToFile(path);
			}
		}

		public Dictionary<int, MapData> maps = new Dictionary<int, MapData>();

		public RaidsData overworldRaids;
		public RaidsData netherRaids;
		public RaidsData endRaids;

		public static WorldData FromWorldSave(string worldSaveDir)
		{
			var wd = new WorldData();
			if(TryLoad(Path.Combine(worldSaveDir, "data", "raids.dat"), out var file)) wd.overworldRaids = RaidsData.Load(file);
			if(TryLoad(Path.Combine(worldSaveDir, "DIM-1", "data", "raids.dat"), out file)) wd.netherRaids = RaidsData.Load(file);
			if(TryLoad(Path.Combine(worldSaveDir, "DIM1", "data", "raids_end.dat"), out file)) wd.endRaids = RaidsData.Load(file);
			//TODO: load maps
			return wd;
		}

		public void Save(string worldSaveDir, GameVersion version)
		{
			if(!Directory.Exists(worldSaveDir)) throw new ArgumentException($"World save directory not found: '{worldSaveDir}'");
			TrySave(overworldRaids, Path.Combine(worldSaveDir, "data", "raids.dat"), version);
			TrySave(netherRaids, Path.Combine(worldSaveDir, "DIM-1", "data", "raids.dat"), version);
			TrySave(endRaids, Path.Combine(worldSaveDir, "DIM1", "data", "raids_end.dat"), version);
			//TODO: save maps
		}

		private static bool TryLoad(string path, out NBTFile file)
		{
			if(File.Exists(path))
			{
				file = new NBTFile(path);
				return true;
			}
			file = null;
			return false;
		}

		private static void TrySave(IData d, string path, GameVersion version)
		{
			if(d == null) return;
			d.Save(path, version);
		}
	}
}

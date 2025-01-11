using System;
using System.Collections.Generic;
using System.IO;
using WorldForge.Maps;
using WorldForge.NBT;

namespace WorldForge
{
	public class WorldData
	{
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

			public void Save(string worldRoot, int id, GameVersion version)
			{
				string path;
				switch(id)
				{
					case 0: path = Path.Combine(worldRoot, "data", "raids.dat"); break;
					case -1: path = Path.Combine(worldRoot, "DIM-1", "data", "raids.dat"); break;
					case 1: path = Path.Combine(worldRoot, "DIM1", "data", "raids_end.dat"); break;
					default: path = Path.Combine(worldRoot, "data", $"raids_DIM{id}.dat"); break;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var file = ToNBT(version);
				file.Save(path);
			}
		}

		public Dictionary<int, IMapData> maps = new Dictionary<int, IMapData>();

		public RaidsData overworldRaids;
		public RaidsData netherRaids;
		public RaidsData endRaids;

		public static WorldData FromWorldSave(string worldSaveDir)
		{
			if(!Directory.Exists(worldSaveDir)) throw new ArgumentException($"World save directory not found: '{worldSaveDir}'");
			var wd = new WorldData();
			if(TryLoad(Path.Combine(worldSaveDir, "data", "raids.dat"), out var file)) wd.overworldRaids = RaidsData.Load(file);
			if(TryLoad(Path.Combine(worldSaveDir, "DIM-1", "data", "raids.dat"), out file)) wd.netherRaids = RaidsData.Load(file);
			if(TryLoad(Path.Combine(worldSaveDir, "DIM1", "data", "raids_end.dat"), out file)) wd.endRaids = RaidsData.Load(file);
			if(Directory.Exists(Path.Combine(worldSaveDir, "data")))
			{
				foreach(var mapFile in Directory.GetFiles(Path.Combine(worldSaveDir, "data"), "map_*.dat"))
				{
					int id = int.Parse(Path.GetFileNameWithoutExtension(mapFile).Substring(4));
					wd.maps.Add(id, new UnloadedMapData(worldSaveDir, id));
				}
			}
			return wd;
		}

		public bool HasMap(int id) => maps.ContainsKey(id);

		public MapData GetMap(int id)
		{
			if(maps.TryGetValue(id, out var i))
			{
				MapData map;
				if(i is UnloadedMapData u)
				{
					map = u.Load();
				}
				else
				{
					map = (MapData)i;
				}
				return map;
			}
			return null;
		}

		public void SetMap(int id, IMapData map) => maps[id] = map;

		public void RemoveMap(int id) => maps.Remove(id);

		public void Save(string worldSaveDir, GameVersion version)
		{
			if(!Directory.Exists(worldSaveDir)) throw new ArgumentException($"World save directory not found: '{worldSaveDir}'");
			TrySave(overworldRaids, worldSaveDir, 0, version);
			TrySave(netherRaids, worldSaveDir, -1, version);
			TrySave(endRaids, worldSaveDir, 1, version);
			foreach(var map in maps)
			{
				map.Value.Save(worldSaveDir, map.Key, version);
			}
			//TODO: save idcounts.dat
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

		private static void TrySave(IData d, string worldSaveRoot, int id, GameVersion version)
		{
			if(d == null) return;
			d.Save(worldSaveRoot, id, version);
		}
	}
}

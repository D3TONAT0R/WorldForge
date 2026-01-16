using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldForge.Maps;
using WorldForge.NBT;

namespace WorldForge
{
	public class WorldData
	{
		public class Raid : INBTConverter
		{
			[NBT("Id")]
			public int id;
			[NBT("Active")]
			public bool active;
			[NBT("Started")]
			public bool started;
			[NBT("Status")]
			public string status;
			[NBT("GroupsSpawned")]
			public int groupsSpawned;
			[NBT("BadOmenLevel")]
			public int badOmenLevel;
			[NBT("PreRaidTicks")]
			public int preRaidTicks;
			[NBT("PostRaidTicks")]
			public int postRaidTicks;
			[NBT("CX")]
			public int cx;
			[NBT("CY")]
			public int cy;
			[NBT("CZ")]
			public int cz;
			[NBT("NumGroups")]
			public int numGroups;
			[NBT("TicksActive")]
			public int ticksActive;
			[NBT("TotalHealth")]
			public float totalHealth;
			[NBT("HeroesOfTheVillage", "1.14")]
			public List<UUID> heroesOfTheVillage = new List<UUID>();

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT(nbtData as NBTCompound, this);
			}
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
				NBTConverter.LoadFromNBT(file.contents, data);
				return data;
			}

			public NBTCompound ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
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
				var file = new NBTFile(ToNBT(version), version.GetDataVersion());
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
					try
					{
						int id = int.Parse(Path.GetFileNameWithoutExtension(mapFile).Substring(4));
						wd.maps.Add(id, new MapDataFile(worldSaveDir, id));
					}
					catch
					{

					}
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
				if(i is MapDataFile u)
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

		public int AddMap(IMapData map)
		{
			for(int i = 0; i < int.MaxValue; i++)
			{
				if(!maps.ContainsKey(i))
				{
					maps.Add(i, map);
					return i;
				}
			}
			throw new InvalidOperationException("Maximum number of maps reached.");
		}

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
			if(maps.Keys.Count > 0)
			{
				int lastMap = maps.Keys.Max();
				var file = new NBTFile(version.GetDataVersion());
				file.contents.Add("map", lastMap);
				file.Save(Path.Combine(worldSaveDir, "data", "idcounts.dat"));
			}
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

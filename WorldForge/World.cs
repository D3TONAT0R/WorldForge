using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.NBT;
using WorldForge.Regions;
using WorldForge.TileEntities;

namespace WorldForge
{
	public class World
	{

		public static readonly BlockID DEFAULT_BLOCK = BlockList.Find("minecraft:stone");

		public GameVersion GameVersion { get; set; }
		public LevelData LevelData { get; private set; }

		public Dimension Overworld { get; set; }
		public Dimension Nether { get; set; }
		public Dimension TheEnd { get; set; }

		public Dictionary<string, PlayerData> playerData = new Dictionary<string, PlayerData>();

		public Dictionary<string, NBTCompound> commandStorage = new Dictionary<string, NBTCompound>();
		public NBTCompound idCounts;
		public NBTCompound raids;
		public Dictionary<int, NBTFile> maps = new Dictionary<int, NBTFile>();

		public string WorldName
		{
			get => LevelData.worldName;
			set => LevelData.worldName = value;
		}

		public bool HasOverworld => Overworld != null;

		public bool HasNether => Nether != null;

		public bool HasTheEnd => TheEnd != null;

		public static World CreateNew(GameVersion version, string worldName)
		{
			var world = new World(version, LevelData.CreateNew());
			world.LevelData.worldName = worldName;
			world.Overworld = Dimension.CreateOverworld(world);
			return world;
		}

		private World(GameVersion targetVersion, LevelData levelData)
		{
			GameVersion = targetVersion;
			this.LevelData = levelData;
		}

		private World()
		{

		}

		//TODO: Danger, loads entire world at once
		public static World Load(string worldSaveDir, GameVersion? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var world = new World();
			world.LevelData = LevelData.Load(new NBTFile(Path.Combine(worldSaveDir, "level.dat")));
			world.GameVersion = GameVersion.FromDataVersion(world.LevelData.dataVersion) ?? gameVersion ?? GameVersion.FirstVersion;

			//Load the dimensions
			world.Overworld = Dimension.Load(world, worldSaveDir, null, DimensionID.Overworld, gameVersion, throwOnRegionLoadFail);
			if(Directory.Exists(Path.Combine(worldSaveDir, "DIM-1")))
			{
				world.Nether = Dimension.Load(world, worldSaveDir, "DIM-1", DimensionID.Nether, gameVersion, throwOnRegionLoadFail);
			}
			if(Directory.Exists(Path.Combine(worldSaveDir, "DIM1")))
			{
				world.TheEnd = Dimension.Load(world, worldSaveDir, "DIM1", DimensionID.TheEnd, gameVersion, throwOnRegionLoadFail);
			}

			return world;
		}

		private static void LoadAlphaChunkFiles(string worldSaveDir, GameVersion? version, bool throwOnRegionLoadFail, Dimension dim)
		{
			var cs = ChunkSerializer.GetOrCreateSerializer<ChunkSerializerAlpha>(version ?? new GameVersion(GameVersion.Stage.Infdev, 1, 0, 0));
			foreach(var f in Directory.GetFiles(worldSaveDir, "c.*.dat"))
			{
				string filename = Path.GetFileName(f);
				try
				{
					var split = filename.Split('.');
					string xBase36 = split[1];
					string zBase36 = split[2];
					var chunkPos = new ChunkCoord(Convert.ToInt32(xBase36, 36), Convert.ToInt32(zBase36, 36));
					var regionPos = new RegionLocation(chunkPos.x >> 5, chunkPos.z >> 5);
					if(!dim.TryGetRegion(regionPos.x, regionPos.z, out var region))
					{
						region = new Region(regionPos, dim);
						dim.regions.Add(regionPos, region);
					}
					//TODO: find a way to enable late loading of alpha chunks
					var nbt = new NBTFile(f);
					var chunk = ChunkData.CreateFromNBT(region, chunkPos.LocalRegionPos, nbt);
					cs.ReadChunkNBT(chunk, version);
				}
				catch(Exception e) when(!throwOnRegionLoadFail)
				{
					Console.WriteLine($"Failed to load region '{filename}': {e.Message}");
				}
			}
		}

		private static void LoadRegionFiles(string worldSaveDir, GameVersion? gameVersion, bool throwOnRegionLoadFail, Dimension dim)
		{
			foreach(var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				var filename = Path.GetFileName(f);
				if(Regex.IsMatch(filename, @"^r.-*\d.-*\d.mc(a|r)"))
				{
					try
					{
						Region region = RegionLoader.LoadRegion(f, gameVersion);
						region.ParentDimension = dim;
						dim.regions.Add(region.regionPos, region);
					}
					catch(Exception e) when(!throwOnRegionLoadFail)
					{
						Console.WriteLine($"Failed to load region '{filename}': {e.Message}");
					}
				}
				else
				{
					Console.WriteLine($"Invalid file '{filename}' in region folder.");
				}
			}
		}

		public static void GetWorldInfo(string worldSaveDir, out string worldName, out GameVersion gameVersion, out RegionLocation[] regions)
		{
			NBTFile levelDat = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			var dataComp = levelDat.contents.GetAsCompound("Data");

			gameVersion = GameVersion.FromDataVersion(dataComp.Get<int>("DataVersion")) ?? GameVersion.FirstVersion;
			worldName = dataComp.Get<string>("LevelName");
			var regionList = new List<RegionLocation>();
			foreach(var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				try
				{
					regionList.Add(RegionLocation.FromRegionFileName(f));
				}
				catch
				{

				}
			}
			regions = regionList.ToArray();
		}

		public void WriteWorldSave(string path, bool createRegionCopyDir = false)
		{
			Directory.CreateDirectory(path);

			var level = CreateLevelDAT(true);

			File.WriteAllBytes(Path.Combine(path, "level.dat"), level.WriteBytesGZip());

			if(HasOverworld)
			{
				Overworld.WriteData(path, GameVersion, createRegionCopyDir);
			}
			if(HasNether)
			{
				Nether.WriteData(Path.Combine(path, "DIM-1"), GameVersion, createRegionCopyDir);
			}
			if(HasTheEnd)
			{
				TheEnd.WriteData(Path.Combine(path, "DIM1"), GameVersion, createRegionCopyDir);
			}
		}

		private NBTFile CreateLevelDAT(bool creativeModeWithCheats)
		{
			var serializer = LevelDATSerializer.CreateForVersion(GameVersion);
			var nbt = new NBTFile();
			serializer.WriteLevelDAT(this, nbt, creativeModeWithCheats);
			return nbt;
		}
	}
}

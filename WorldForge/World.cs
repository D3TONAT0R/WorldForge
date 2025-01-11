using System;
using System.Collections.Generic;
using System.IO;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.NBT;

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

		public WorldData WorldData { get; private set; }

		public string WorldName
		{
			get => LevelData.worldName;
			set => LevelData.worldName = value;
		}

		public bool HasOverworld => Overworld != null;

		public bool HasNether => Nether != null;

		public bool HasTheEnd => TheEnd != null;

		public static World CreateNew(GameVersion version, string worldName, bool createOverworld = true)
		{
			var world = new World(version, LevelData.CreateNew(), new WorldData());
			world.LevelData.worldName = worldName;
			if(createOverworld)
			{
				world.Overworld = Dimension.CreateOverworld(world);
			}
			return world;
		}

		public static World Load(string worldSaveDir, GameVersion? versionHint = null, bool throwOnRegionLoadFail = false)
		{
			var world = new World();
			world.LevelData = LevelData.Load(new NBTFile(Path.Combine(worldSaveDir, "level.dat")), versionHint, out var actualGameVersion);
			world.GameVersion = actualGameVersion ?? versionHint ?? GameVersion.FirstVersion;

			//Load the dimensions
			world.Overworld = Dimension.Load(world, worldSaveDir, null, DimensionID.Overworld, versionHint, throwOnRegionLoadFail);
			if(Directory.Exists(Path.Combine(worldSaveDir, "DIM-1")))
			{
				world.Nether = Dimension.Load(world, worldSaveDir, "DIM-1", DimensionID.Nether, versionHint, throwOnRegionLoadFail);
			}
			if(Directory.Exists(Path.Combine(worldSaveDir, "DIM1")))
			{
				world.TheEnd = Dimension.Load(world, worldSaveDir, "DIM1", DimensionID.TheEnd, versionHint, throwOnRegionLoadFail);
			}

			world.WorldData = WorldData.FromWorldSave(worldSaveDir);

			return world;
		}

		private World(GameVersion targetVersion, LevelData levelData, WorldData worldData)
		{
			GameVersion = targetVersion;
			LevelData = levelData;
			WorldData = worldData;
		}

		private World()
		{

		}

		public void PlaceSpawnpoint(int x, int z, bool throwException = false)
		{
			short y = Overworld.GetHighestBlock(x, z, HeightmapType.AllBlocks);
			if(y == short.MinValue)
			{
				if(throwException) throw new InvalidOperationException("Could not find any blocks at the given spawn location");
				y = 64;
			}
			LevelData.spawnpoint = new LevelData.Spawnpoint(x, y, z);
		}

		public void SetGameMode(Player.GameMode gameMode, bool forceAllPlayers = false)
		{
			LevelData.gameTypeAndDifficulty.gameType = gameMode;
			LevelData.gameTypeAndDifficulty.allowCommands = true;
			if(forceAllPlayers)
			{
				LevelData.player.playerGameType = gameMode;
				foreach(var player in playerData.Values)
				{
					player.player.playerGameType = gameMode;
				}
			}
		}

		public static void GetWorldInfo(string worldSaveDir, out string worldName, out GameVersion gameVersion, out List<RegionLocation> regions)
		{
			NBTFile levelDat = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			var dataComp = levelDat.contents.GetAsCompound("Data");

			gameVersion = GameVersion.FromDataVersion(dataComp.Get<int>("DataVersion")) ?? GameVersion.FirstVersion;
			worldName = dataComp.Get<string>("LevelName");
			regions = new List<RegionLocation>();
			foreach(var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				if(RegionLocation.TryGetFromFileName(f, out var loc))
				{
					regions.Add(loc);
				}
			}
		}

		public void WriteWorldSave(string path, bool createSpawnpointMapIcon = true)
		{
			Directory.CreateDirectory(path);

			var level = CreateLevelDAT(true);

			File.WriteAllBytes(Path.Combine(path, "level.dat"), level.WriteBytesGZip(false));

			if(HasOverworld)
			{
				Overworld.WriteData(path, GameVersion);
			}
			if(HasNether)
			{
				Nether.WriteData(Path.Combine(path, "DIM-1"), GameVersion);
			}
			if(HasTheEnd)
			{
				TheEnd.WriteData(Path.Combine(path, "DIM1"), GameVersion);
			}

			WorldData.Save(path, GameVersion);

			if(createSpawnpointMapIcon)
			{
				var spawnX = LevelData.spawnpoint.spawnX;
				var spawnZ = LevelData.spawnpoint.spawnZ;
				var icon = SurfaceMapGenerator.GenerateSurfaceMap(Overworld, new Boundary(spawnX - 32, spawnZ - 32, spawnX + 32, spawnZ + 32), HeightmapType.AllBlocks, true);
				icon.Save(Path.Combine(path, "icon.png"));
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

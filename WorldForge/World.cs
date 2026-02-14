using System;
using System.Collections.Generic;
using System.IO;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.NBT;

namespace WorldForge
{
	public enum SaveFileOption
	{
		/// <summary>
		/// Creates new save files without overwriting.
		/// </summary>
		CreateNew = 0,
		/// <summary>
		/// Creates a new save file or replaces existing saves.
		/// </summary>
		Replace = 1,
		/// <summary>
		/// Creates a new save file or replaces existing files (including non-save files).
		/// </summary>
		ReplaceAny = 2,
		/// <summary>
		/// Creates a new save file or overwrites existing files, keeping the original save intact.
		/// </summary>
		Update = 3
	}

	public class World
	{
		public string SourceDirectory { get; private set; }

		public GameVersion GameVersion { get; set; }
		public LevelData LevelData { get; private set; }

		public Dictionary<DimensionID, Dimension> Dimensions { get; } = new Dictionary<DimensionID, Dimension>();

		public Dimension Overworld
		{
			get => GetDimension(DimensionID.Overworld);
			set => Dimensions[DimensionID.Overworld] = value;
		}
		public Dimension Nether
		{
			get => GetDimension(DimensionID.Nether);
			set => Dimensions[DimensionID.Nether] = value;
		}
		public Dimension TheEnd
		{
			get => GetDimension(DimensionID.TheEnd);
			set => Dimensions[DimensionID.TheEnd] = value;
		}

		public bool HasOverworld => Overworld != null;

		public bool HasNether => Nether != null;

		public bool HasTheEnd => TheEnd != null;

		public Dictionary<UUID, PlayerData> PlayerData { get; } = new Dictionary<UUID, PlayerData>();

		public WorldData WorldData { get; private set; }

		public string WorldName
		{
			get => LevelData.worldName;
			set => LevelData.worldName = value;
		}

		public static World Create(GameVersion version, string name, bool createOverworld = true)
		{
			var world = new World(version, LevelData.Create(), WorldData.Create());
			world.LevelData.worldName = name;
			if (createOverworld)
			{
				world.Overworld = Dimension.Create(world, DimensionID.Overworld, BiomeID.Plains);
			}
			return world;
		}

		public static World CreatePlaceholder(string name = "Placeholder")
		{
			return Create(GameVersion.DefaultVersion, name);
		}

		public static World Load(string worldSaveDir, GameVersion? versionHint = null, bool throwOnRegionLoadFail = false)
		{
			var world = new World();
			world.SourceDirectory = worldSaveDir;

			world.LevelData = LevelData.Load(new NBTFile(Path.Combine(worldSaveDir, "level.dat")), versionHint, out var actualGameVersion);
			world.GameVersion = actualGameVersion ?? versionHint ?? GameVersion.FirstAnvilVersion;

			//Load the dimensions
			world.Overworld = Dimension.Load(world, null, DimensionID.Overworld, versionHint, throwOnRegionLoadFail);
			foreach (var dir in Directory.GetDirectories(worldSaveDir, "DIM*"))
			{
				var dirName = Path.GetFileName(dir);
				if (int.TryParse(dirName.Substring(3), out var dimIndex))
				{
					var dimensionID = DimensionID.FromIndex(dimIndex);
					world.Dimensions[dimensionID] = Dimension.Load(world, dirName, dimensionID, versionHint, throwOnRegionLoadFail);
				}
			}
			string dimensionsDir = Path.Combine(worldSaveDir, "dimensions");
			if (Directory.Exists(dimensionsDir))
			{
				foreach (var dir1 in Directory.GetDirectories(dimensionsDir))
				{
					var ns = Path.GetFileName(dir1);
					foreach (var dir2 in Directory.GetDirectories(dir1))
					{
						var name = Path.GetFileName(dir2);
						var dimensionID = DimensionID.FromID($"{ns}:{name}");
						world.Dimensions[dimensionID] = Dimension.Load(world, Path.Combine("dimensions", ns, name), dimensionID, versionHint, throwOnRegionLoadFail);
					}
				}
			}

			world.WorldData = WorldData.FromWorldSave(worldSaveDir);

			if (Directory.Exists(Path.Combine(worldSaveDir, "playerdata")))
			{
				foreach (var file in Directory.GetFiles(Path.Combine(worldSaveDir, "playerdata"), "*.dat"))
				{
					try
					{
						var uuid = new UUID(Path.GetFileNameWithoutExtension(file));
						var player = new PlayerData(worldSaveDir, uuid, world.GameVersion);
						world.PlayerData[uuid] = player;
					}
					catch (Exception e)
					{
						Logger.Error($"Failed to load player data from file {file}: {e.Message}");
					}
				}
			}

			return world;
		}

		public static bool IsWorldSaveDirectory(string directory)
		{
			return File.Exists(Path.Combine(directory, "level.dat")) || Directory.Exists(Path.Combine(directory, "region"));
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

		public Dimension GetDimension(DimensionID dimensionID)
		{
			if (Dimensions.TryGetValue(dimensionID, out var dimension))
			{
				return dimension;
			}
			return null;
		}

		public bool HasDimension(DimensionID dimensionID)
		{
			return Dimensions.ContainsKey(dimensionID);
		}

		public bool TryGetDimension(DimensionID dimensionID, out Dimension dimension)
		{
			return Dimensions.TryGetValue(dimensionID, out dimension);
		}

		public void PlaceSpawnpoint(int x, int z, bool throwException = false)
		{
			short y = Overworld.GetHighestBlock(x, z, HeightmapType.AllBlocks);
			if (y == short.MinValue)
			{
				if (throwException) throw new InvalidOperationException("Could not find any blocks at the given spawn location");
				y = 64;
			}
			LevelData.spawnpoint = new LevelData.Spawnpoint(x, y, z);
		}

		public void SetGameMode(Player.GameMode gameMode, bool forceAllPlayers = false)
		{
			LevelData.gameTypeAndDifficulty.gameType = gameMode;
			LevelData.gameTypeAndDifficulty.allowCommands = true;
			if (forceAllPlayers)
			{
				LevelData.player.playerGameType = gameMode;
				foreach (var player in PlayerData.Values)
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
			foreach (var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				if (RegionLocation.TryGetFromFileName(f, out var loc))
				{
					regions.Add(loc);
				}
			}
		}

		public void Save(string worldSaveDir, bool createSpawnpointMapIcon = true, SaveFileOption saveFileOption = SaveFileOption.Replace)
		{
			bool exists = Directory.Exists(worldSaveDir);
			bool hasFiles = exists && Directory.GetFiles(worldSaveDir, "*", SearchOption.AllDirectories).Length > 0;

			if (saveFileOption == SaveFileOption.CreateNew && hasFiles)
			{
				throw new InvalidOperationException("Target directory already contains files.");
			}
			else if (saveFileOption == SaveFileOption.Replace && hasFiles)
			{
				//Check if the destination folder is a world folder to avoid unintended deletions
				if (!IsWorldSaveDirectory(worldSaveDir))
				{
					throw new ArgumentException("Target directory already contains files and is not a world save.");
				}
				Directory.Delete(worldSaveDir, true);
			}
			else if (saveFileOption == SaveFileOption.ReplaceAny)
			{
				Directory.Delete(worldSaveDir, true);
			}

			Directory.CreateDirectory(worldSaveDir);

			SaveLevelData(worldSaveDir);

			if (HasOverworld)
			{
				Overworld.SaveFiles(worldSaveDir, GameVersion);
			}
			if (HasNether)
			{
				Nether.SaveFiles(Path.Combine(worldSaveDir, "DIM-1"), GameVersion);
			}
			if (HasTheEnd)
			{
				TheEnd.SaveFiles(Path.Combine(worldSaveDir, "DIM1"), GameVersion);
			}

			WorldData.Save(worldSaveDir, GameVersion);

			if (createSpawnpointMapIcon && Overworld != null)
			{
				var spawnX = LevelData.spawnpoint.spawnX;
				var spawnZ = LevelData.spawnpoint.spawnZ;
				var icon = SurfaceMapGenerator.GenerateSurfaceMap(Overworld, new Boundary(spawnX - 32, spawnZ - 32, spawnX + 32, spawnZ + 32), HeightmapType.AllBlocks, true);
				icon.Save(Path.Combine(worldSaveDir, "icon.png"));
			}
		}

		private void SaveLevelData(string worldSaveDir)
		{
			var serializer = LevelDATSerializer.CreateForVersion(GameVersion);
			var nbt = serializer.CreateNBTFile(this);
			nbt.Save(Path.Combine(worldSaveDir, "level.dat"), false);
		}

		public string GetDimensionDirectory(string worldSaveDir, DimensionID dimensionID, bool forceDimensionsDirectory)
		{
			var subdir = dimensionID.GetSubdirectoryName(forceDimensionsDirectory); ;
			if (subdir != null)
			{
				return Path.Combine(worldSaveDir, subdir);
			}
			else
			{
				return worldSaveDir;
			}
		}
	}
}

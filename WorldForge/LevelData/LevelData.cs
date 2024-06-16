using System;
using System.Collections.Generic;
using System.Reflection;
using WorldForge.Biomes;
using WorldForge.NBT;

namespace WorldForge
{
	/// <summary>
	/// Data structure containing various information about the world (the level.dat file).
	/// </summary>
	public partial class LevelData
	{
		/// <summary>
		/// Represents the location of a spawnpoint in the world.
		/// </summary>
		public class Spawnpoint
		{
			/// <summary>
			/// The X coordinate of the spawnpoint.
			/// </summary>
			[NBT("SpawnX")]
			public int spawnX = 0;
			/// <summary>
			/// The Y coordinate of the spawnpoint.
			/// </summary>
			[NBT("SpawnY")]
			public int spawnY = 0;
			/// <summary>
			/// The Z coordinate of the spawnpoint.
			/// </summary>
			[NBT("SpawnZ")]
			public int spawnZ = 0;
			/// <summary>
			/// The angle at which the player will spawn. (1.16+)
			/// </summary>
			[NBT("SpawnAngle", "1.16")]
			public float spawnAngle = 0f;

			public Spawnpoint()
			{

			}

			public Spawnpoint(int x, int y, int z, float angle = 0)
			{
				spawnX = x;
				spawnY = y;
				spawnZ = z;
				spawnAngle = angle;
			}

			public Spawnpoint(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			/// <summary>
			/// Sets the spawnpoint to the specified coordinates.
			/// </summary>
			public void Set(int x, int y, int z, float? angle = 0f)
			{
				spawnX = x;
				spawnY = y;
				spawnZ = z;
				spawnAngle = angle ?? spawnAngle;
			}

			/// <summary>
			/// Sets the spawnpoint to the specified coordinates on the surface of the world.
			/// </summary>
			public void SetOnSurface(int x, int z, World w)
			{
				spawnX = x;
				spawnZ = z;
				spawnY = w.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
			}
		}

		/// <summary>
		/// Data related to the game type and difficulty of the world.
		/// </summary>
		public class GameTypeAndDifficulty
		{
			//TODO: find out when all of these were added
			/// <summary>
			/// If true, commands (cheats) are allowed in the world. (1.3.1+)
			/// </summary>
			[NBT("allowCommands", "1.3.1")]
			public bool allowCommands = false;
			/// <summary>
			/// The default game mode for new players joining the world. This does not affect existing players. (1.0.0+)
			/// </summary>
			[NBT("GameType", "1.0.0")]
			public Player.GameMode gameType = Player.GameMode.Survival;
			/// <summary>
			/// The difficulty level of the world. (1.8+)
			/// </summary>
			[NBT("Difficulty", "1.8")]
			public DifficultyLevel difficulty = DifficultyLevel.Easy;
			/// <summary>
			/// If true, the difficulty level of the world is locked and cannot be changed. (1.8+)
			/// </summary>
			[NBT("DifficultyLocked", "1.8")]
			public bool difficultyLocked = false;
			/// <summary>
			/// If true, the world is in hardcore mode. (1.0.0+)
			/// </summary>
			[NBT("hardcore", "1.0.0")]
			public bool hardcoreMode = false;

			public GameTypeAndDifficulty()
			{

			}

			public GameTypeAndDifficulty(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			public void WriteToNBT(NBTCompound nbt, GameVersion version)
			{
				NBTConverter.WriteToNBT(this, nbt, version);
			}
		}

		public enum DifficultyLevel { Peaceful, Easy, Medium, Hard }

		public class WorldBorder
		{
			[NBT("BorderCenterX")]
			public double borderCenterX = 0.0d;
			[NBT("BorderCenterZ")]
			public double borderCenterZ = 0.0d;
			[NBT("BorderDamagePerBlock")]
			public double borderDamagePerBlock = 0.2d;
			[NBT("BorderSafeZone")]
			public double borderSafeZone = 5.0d;
			[NBT("BorderSize")]
			public double borderSize = 59999968.0d;
			[NBT("BorderSizeLerpTarget")]
			public double borderSizeLerpTarget = 59999968.0d;
			[NBT("BorderSizeLerpTime")]
			public long borderSizeLerpTime = 0;
			[NBT("BorderWarningBlocks")]
			public double borderWarningBlocks = 5.0d;
			[NBT("BorderWarningTime")]
			public double borderWarningTime = 15.0d;

			public WorldBorder() { }

			public WorldBorder(NBTCompound levelDat)
			{
				NBTConverter.LoadFromNBT(levelDat, this);
			}
		}

		public class TimeAndWeather
		{
			private static Random random = new Random();

			/// <summary>
			/// The number of ticks since the start of the level. Does not represent the time of day.
			/// </summary>
			[NBT("Time")]
			private long timeSinceStart = 0;
			//TODO: find version where this was added
			[NBT("DayTime", "1.0.0")]
			private long worldTime = 0;

			public long WorldTime
			{
				get => timeSinceStart;
				set
				{
					timeSinceStart = value;
					worldTime = value;
				}
			}

			public long TimeSinceLevelStart
			{
				get => timeSinceStart;
				set => timeSinceStart = value;
			}

			public long TimeOfDay => worldTime % 24000;

			[NBT(null, "1.8")]
			public int clearWeatherTime = 0;
			[NBT]
			public bool raining = false;
			[NBT]
			public int rainTime = 12000;
			[NBT]
			public bool thundering = false;
			[NBT]
			public int thunderTime = 120000;

			public TimeAndWeather()
			{

			}

			public TimeAndWeather(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			public void SetTimeSunrise()
			{
				if(TimeOfDay == 0) return;
				WorldTime += 24000 - TimeOfDay;
			}

			public void SetTimeMidday()
			{
				if(TimeOfDay == 6000) return;
				SetTimeSunrise();
				WorldTime += 6000;
			}

			public void SetTimeSunset()
			{
				if(TimeOfDay == 12000) return;
				SetTimeSunrise();
				WorldTime += 12000;
			}

			public void SetTimeMidnight()
			{
				if(TimeOfDay == 18000) return;
				SetTimeSunrise();
				WorldTime += 18000;
			}

			public void SetClearWeather(int? fixedDuration = null)
			{
				int duration = fixedDuration ?? random.Next(12000, 180000);
				clearWeatherTime = 0;
				raining = false;
				rainTime = duration;
			}

			public void SetRain(int? fixedDuration = null)
			{
				int duration = fixedDuration ?? random.Next(12000, 24000);
				rainTime = 0;
				raining = true;
				clearWeatherTime = duration;
			}

			public void SetThunderstorm(int? fixedDuration = null)
			{
				//TODO: needs check if this actually works
				SetRain(fixedDuration);
				thundering = true;
				thunderTime = 0;
			}
		}

		public class WanderingTraderInfo
		{
			[NBT("WanderingTraderSpawnChance")]
			public int spawnChance = 25;
			[NBT("WanderingTraderSpawnDelay")]
			public int spawnDelay = 24000;

			public WanderingTraderInfo()
			{

			}

			public WanderingTraderInfo(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}
		}

		public class DataPacks
		{
			[NBT("Disabled")]
			public NBTList disabled = new NBTList(NBTTag.TAG_String);
			[NBT("Enabled")]
			public NBTList enabled = new NBTList(NBTTag.TAG_String);

			public DataPacks()
			{
				enabled.Add("vanilla");
			}

			public DataPacks(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}
		}

		//TODO: find out when this was added
		[NBT("initialized")]
		public bool initialized = true;
		/// <summary>
		/// The name of the world.
		/// </summary>
		[NBT("LevelName")]
		public string worldName = "MCUtils generated world " + new Random().Next(10000);
		[NBT("DataVersion")]
		public int dataVersion;
		/// <summary>
		/// The time the world was last played, in Unix timestamp format.
		/// </summary>
		[NBT("LastPlayed")]
		public long lastPlayedUnixTimestamp;
		//TODO: find out when this was added
		[NBT("enabled_features", "1.19")]
		public List<string> enabledFeatures;
		//TODO: find out when this was added
		[NBT("WasModded", "1.13.0")]
		public bool wasModded;

		public Player player = new Player(new Vector3(0, 0, 0));

		/// <summary>
		/// The world spawnpoint where players will spawn.
		/// </summary>
		public Spawnpoint spawnpoint = new Spawnpoint();
		public GameTypeAndDifficulty gameTypeAndDifficulty = new GameTypeAndDifficulty();
		public TimeAndWeather timeAndWeather = new TimeAndWeather();
		public GameRules gameRules = new GameRules();
		public WorldGenerator worldGen = new WorldGenerator();
		public WorldBorder worldBorder = new WorldBorder();
		public WanderingTraderInfo wanderingTraderInfo = new WanderingTraderInfo();
		public DataPacks dataPacks = new DataPacks();

		private LevelData() { }

		public static LevelData CreateNew()
		{
			return new LevelData();
		}

		public static LevelData Load(NBTFile levelDat)
		{
			var levelNBT = levelDat.contents.Get<NBTCompound>("Data");
			var d = new LevelData();
			NBTConverter.LoadFromNBT(levelNBT, d);
			if(levelNBT.TryGet<NBTCompound>("Player", out var playerComp))
			{
				d.player = new Player(playerComp);
			}
			d.spawnpoint = new Spawnpoint(levelNBT);
			d.gameTypeAndDifficulty = new GameTypeAndDifficulty(levelNBT);
			d.timeAndWeather = new TimeAndWeather(levelNBT);
			if(levelNBT.TryGet<NBTCompound>("GameRules", out var rulesComp))
			{
				d.gameRules = new GameRules(rulesComp);
			}
			if(levelNBT.TryGet<NBTCompound>("WorldGenSettings", out var wg))
			{
				d.worldGen = new WorldGenerator(wg);
			}
			d.worldBorder = new WorldBorder(levelNBT);
			d.wanderingTraderInfo = new WanderingTraderInfo(levelNBT);
			if(levelNBT.TryGet<NBTCompound>("DataPacks", out var dp))
			{
				d.dataPacks = new DataPacks(dp);
			}
			return d;
		}

		public static int GetNBTVersion(GameVersion gameVersion)
		{
			if(gameVersion >= GameVersion.FirstAnvilVersion) return 19133;
			else if(gameVersion >= GameVersion.FirstMCRVersion) return 19132;
			else return 10;
		}
	}
}
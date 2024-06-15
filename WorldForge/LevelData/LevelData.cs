using System;
using System.Collections.Generic;
using System.Reflection;
using WorldForge.Biomes;
using WorldForge.NBT;

namespace WorldForge
{
	public partial class LevelData
	{
		public class Spawnpoint
		{
			[NBT("SpawnX")]
			public int spawnX = 0;
			[NBT("SpawnY")]
			public int spawnY = -1;
			[NBT("SpawnZ")]
			public int spawnZ = 0;
			[NBT("SpawnAngle")]
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

			public void Set(int x, int y, int z, float? angle = 0f)
			{
				spawnX = x;
				spawnY = y;
				spawnZ = z;
				spawnAngle = angle ?? spawnAngle;
			}

			public void SetOnSurface(int x, int z, World w)
			{
				spawnX = x;
				spawnZ = z;
				spawnY = w.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
			}
		}

		public class GameTypeAndDifficulty
		{
			[NBT("allowCommands")]
			public bool allowCommands = false;
			[NBT("GameType")]
			public Player.GameMode gameType = Player.GameMode.Survival;
			[NBT("Difficulty")]
			public DifficultyLevel difficulty = DifficultyLevel.Easy;
			[NBT("DifficultyLocked")]
			public bool difficultyLocked = false;
			[NBT("hardcore")]
			public bool hardcoreMode = false;

			public GameTypeAndDifficulty()
			{

			}

			public GameTypeAndDifficulty(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
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

			[NBT("Time")]
			private long timeSinceStart = 0;
			[NBT("DayTime")]
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

			[NBT]
			public int clearWeatherTime = 0;
			[NBT]
			public bool raining = false;
			[NBT]
			public int rainTime = 12000;
			[NBT]
			public bool thundering = false;
			[NBT]
			public int thunderTime = 120000;

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

		[NBT("LevelName")]
		public string worldName = "MCUtils generated world " + new Random().Next(10000);
		[NBT("DataVersion")]
		public int dataVersion;
		[NBT("LastPlayed")]
		public long lastPlayedUnixTimestamp;
		[NBT("enabled_features")]
		public List<string> enabledFeatures;
		[NBT("WasModded")]
		public bool wasModded;

		public Player player = new Player(new Vector3(0, 0, 0));

		public Spawnpoint spawnpoint = new Spawnpoint();
		public GameTypeAndDifficulty gameTypeAndDifficulty = new GameTypeAndDifficulty();
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
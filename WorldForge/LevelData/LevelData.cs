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

		public class GameRules
		{
			[NBT(null, "1.4.2")]
			public bool doFireTick = true;
			[NBT(null, "1.4.2")]
			public bool mobGriefing = true;
			[NBT(null, "1.4.2")]
			public bool keepInventory = false;
			[NBT(null, "1.4.2")]
			public bool doMobSpawning = true;
			[NBT(null, "1.4.2")]
			public bool doMobLoot = true;
			[NBT(null, "1.4.2")]
			public bool doTileDrops = true;
			[NBT(null, "1.4.2")]
			public bool commandBlockOutput = true;

			[NBT(null, "1.6.1")]
			public bool naturalRegeneration = true;
			[NBT(null, "1.6.1")]
			public bool doDaylightCycle = true;

			[NBT(null, "1.8")]
			public bool logAdminCommands = true;
			[NBT(null, "1.8")]
			public bool showDeathMessages = true;
			[NBT(null, "1.8")]
			public int randomTickSpeed = 3;
			[NBT(null, "1.8")]
			public bool sendCommandFeedback = true;
			[NBT(null, "1.8")]
			public bool reducedDebugInfo = false;

			[NBT(null, "1.8.1")]
			public bool doEntityDrops = true;

			[NBT(null, "1.9")]
			public bool spectatorsGenerateChunks = true;
			[NBT(null, "1.9")]
			public int spawnRadius = 10;
			[NBT(null, "1.9")]
			public bool disableElytraMovementCheck = false;

			[NBT(null, "1.11")]
			public bool doWeatherCycle = true;
			[NBT(null, "1.11")]
			public int maxEntityCramming = 24;

			[NBT(null, "1.12")]
			public bool doLimitedCrafting = false;
			[NBT(null, "1.12")]
			public int maxCommandChainLength = 65536;
			[NBT(null, "1.12")]
			public bool announceAdvancements = true;

			[NBT(null, "1.13")]
			public string gameLoopFunction = "";

			[NBT(null, "1.14.3")]
			public bool disableRaids = false;

			[NBT(null, "1.15")]
			public bool doInsomnia = true;
			[NBT(null, "1.15")]
			public bool doImmediateRespawn = false;
			[NBT(null, "1.15")]
			public bool drowningDamage = true;
			[NBT(null, "1.15")]
			public bool fallDamage = true;
			[NBT(null, "1.15")]
			public bool fireDamage = true;

			[NBT(null, "1.15.2")]
			public bool doPatrolSpawning = true;
			[NBT(null, "1.15.2")]
			public bool doTraderSpawning = true;

			[NBT(null, "1.16")]
			public bool universalAnger = false;
			[NBT(null, "1.16")]
			public bool forgiveDeadPlayers = true;

			[NBT(null, "1.17")]
			public bool freezeDamage = true;
			[NBT(null, "1.17")]
			public int playersSleepingPercentage = 100;

			[NBT(null, "1.19")]
			public bool doWardenSpawning = true;

			[NBT(null, "1.19.3")]
			public bool blockExplosionDropDecay = true;
			[NBT(null, "1.19.3")]
			public bool mobExplosionDropDecay = true;
			[NBT(null, "1.19.3")]
			public bool tntExplosionDropDecay = false;
			[NBT(null, "1.19.3")]
			public int snowAccumulationHeight = 1;
			[NBT(null, "1.19.3")]
			public bool waterSourceConversion = true;
			[NBT(null, "1.19.3")]
			public bool lavaSourceConversion = false;
			[NBT(null, "1.19.3")]
			public bool globalSoundEvents = true;

			[NBT(null, "1.19.4")]
			public int commandModificationBlockLimit = 32768;
			[NBT(null, "1.19.4")]
			public bool doVinesSpread = true;

			[NBT(null, "1.20.2")]
			public bool enderPearlsVanishOnDeath = true;

			[NBT(null, "1.20.3")]
			public int maxCommandForkCount = 10000;
			[NBT(null, "1.20.3")]
			public bool projectilesCanBreakBlocks = true;
			[NBT(null, "1.20.3")]
			public int playerNetherPortalDefaultDelay = 80;
			[NBT(null, "1.20.3")]
			public int playerNetherPortalCreativeDelay = 1;

			[NBT(null, "1.20.5")]
			public int spawnChunkRadius = 2;

			public GameRules()
			{

			}

			public GameRules(NBTCompound levelDataNBT)
			{
				if(levelDataNBT.TryGet<NBTCompound>("GameRules", out var gameRulesNBT))
				{
					foreach(var f in typeof(GameRules).GetFields())
					{
						try
						{
							if(gameRulesNBT.TryGet(f.Name, out string nbtValue))
							{
								if(f.FieldType == typeof(bool))
								{
									f.SetValue(this, bool.Parse(nbtValue));
								}
								else if(f.FieldType == typeof(int))
								{
									f.SetValue(this, int.Parse(nbtValue));
								}
								else
								{
									throw new InvalidOperationException("Field type is incompatible, must be either bool or int.");
								}
							}
						}
						catch(Exception e)
						{
							Console.WriteLine($"Failed to load GameRule '{f.Name}': {e.Message}");
						}
					}
				}
			}

			public NBTCompound CreateNBT(GameVersion version)
			{
				var comp = new NBTCompound();
				foreach(var f in typeof(GameRules).GetFields())
				{
					var attribute = f.GetCustomAttribute<NBTAttribute>();
					if(version >= attribute.addedIn && version < attribute.removedIn)
					{
						comp.Add(f.Name, f.GetValue(this).ToString().ToLower());
					}
				}
				return comp;
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
			private long time = 0;
			[NBT("DayTime")]
			private long dayTime = 0;

			public long WorldTime
			{
				get => time;
				set
				{
					time = value;
					dayTime = value;
				}
			}

			public long TimeOfDay => dayTime % 24000;

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
	}
}
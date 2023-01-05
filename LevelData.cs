using MCUtils.NBT;
using System;
using System.Collections.Generic;

namespace MCUtils
{
	public class LevelData
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

			public Spawnpoint(NBTCompound nbt)
			{
				NBTFieldManager.LoadFromNBT(nbt, this);
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
				NBTFieldManager.LoadFromNBT(nbt, this);
			}
		}

		public class GameRules
		{
			public bool announceAdvancements = true;
			public bool commandBlockOutput = true;
			public bool disableElytraMovementCheck = false;
			public bool disableRaids = false;
			public bool doDaylightCycle = true;
			public bool doEntityDrops = true;
			public bool doFireTick = true;
			public bool doImmediateRespawn = false;
			public bool doInsomnia = true;
			public bool doLimitedCrafting = false;
			public bool doMobLoot = true;
			public bool doMobSpawning = true;
			public bool doPatrolSpawning = true;
			public bool doTileDrops = true;
			public bool doTraderSpawning = true;
			public bool doWardenSpawning = true;
			public bool doWeatherCycle = true;
			public bool drowningDamage = true;
			public bool fallDamage = true;
			public bool fireDamage = true;
			public bool forgiveDeadPlayers = true;
			public bool freezeDamage = true;
			public bool keepInventory = false;
			public bool logAdminCommands = true;
			public int maxCommandChainLength = 65536;
			public int maxEntityCramming = 24;
			public bool mobGriefing = true;
			public bool naturalRegeneration = true;
			public int playersSleepingPercentage = 100;
			public int randomTickSpeed = 3;
			public bool reducedDebugInfo = false;
			public bool sendCommandFeedback = true;
			public int spawnRadius = 10;
			public bool spectatorsGenerateChunks = true;
			public bool universalAnger = false;

			public GameRules()
			{

			}

			public GameRules(NBTCompound levelDataNBT)
			{
				if (levelDataNBT.TryGet<NBTCompound>("GameRules", out var gameRulesNBT))
				{
					foreach (var f in typeof(GameRules).GetFields())
					{
						try
						{
							if (gameRulesNBT.TryGet(f.Name, out string nbtValue))
							{
								if (f.FieldType == typeof(bool))
								{
									f.SetValue(this, bool.Parse(nbtValue));
								}
								else if (f.FieldType == typeof(int))
								{
									f.SetValue(this, int.Parse(nbtValue));
								}
								else
								{
									throw new InvalidOperationException("Field type is incompatible, must be either bool or int.");
								}
							}
						}
						catch (Exception e)
						{
							Console.WriteLine($"Failed to load GameRule '{f.Name}': {e.Message}");
						}
					}
				}
			}

			public NBTCompound CreateNBT()
			{
				var comp = new NBTCompound();
				foreach (var f in typeof(GameRules).GetFields())
				{
					comp.Add(f.Name, f.GetValue(this).ToString().ToLower());
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
				NBTFieldManager.LoadFromNBT(levelDat, this);
			}
		}

		public class WorldGenerator
		{
			[NBT("bonus_chest")]
			public bool generateBonusChest = false;
			[NBT("generate_features")]
			public bool generateFeatures = true;
			[NBT("seed")]
			private long seed;
			[NBT("dimensions")]
			public NBTCompound dimensionGenerators;

			public long WorldSeed => seed;

			public WorldGenerator(long seed)
			{
				this.seed = seed;
				dimensionGenerators = new NBTCompound();
				dimensionGenerators.Add("minecraft:overworld", CreateDimensionGenerator("minecraft:overworld", "minecraft:overworld", seed));
				//TODO: needs fixing (the_nether and nether, the_end and end)
				dimensionGenerators.Add("minecraft:the_nether", CreateDimensionGenerator("minecraft:the_nether", "minecraft:nether", seed));
				dimensionGenerators.Add("minecraft:the_end", CreateDimensionGenerator("minecraft:the_end", "minecraft:end", seed));
			}

			public WorldGenerator() : this(new Random().Next(int.MinValue, int.MaxValue))
			{

			}

			public WorldGenerator(NBTCompound nbt)
			{
				NBTFieldManager.LoadFromNBT(nbt, this);
			}

			private NBTCompound CreateDimensionGenerator(string genType, string generator, long seed)
			{
				NBTCompound comp = new NBTCompound() {
					{ "type", genType },
					{ "generator", new NBTCompound() {
						{ "seed", seed },
						{ "settings", generator },
						{ "type", "minecraft:noise" },
						{ "biome_source", new NBTCompound() {
							//Add preset/type/seed here
						} }
					} }
				};
				var bs = comp.GetAsCompound("generator").GetAsCompound("biome_source");
				if (genType != "minecraft:the_end")
				{
					bs.Add("preset", genType);
					bs.Add("type", "minecraft:multi_noise");
				}
				else
				{
					bs.Add("seed", seed);
					bs.Add("type", genType);
				}
				return comp;
			}

			public void SetWorldSeed(long newSeed)
			{
				seed = newSeed;
				foreach(var k in dimensionGenerators.GetContentKeys())
				{
					SetSeed(dimensionGenerators.GetAsCompound(k), newSeed);
				}
			}

			public void SetSeed(NBTCompound dimensionGenerator, long newSeed)
			{
				var gen = dimensionGenerator.GetAsCompound("generator");
				gen.Set("seed", newSeed);
				var bs = gen.GetAsCompound("biome_source");
				if(bs.Contains("seed"))
				{
					bs.Set("seed", newSeed);
				}
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
				if (TimeOfDay == 0) return;
				WorldTime += 24000 - TimeOfDay;
			}

			public void SetTimeMidday()
			{
				if (TimeOfDay == 6000) return;
				SetTimeSunrise();
				WorldTime += 6000;
			}

			public void SetTimeSunset()
			{
				if (TimeOfDay == 12000) return;
				SetTimeSunrise();
				WorldTime += 12000;
			}

			public void SetTimeMidnight()
			{
				if (TimeOfDay == 18000) return;
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
				NBTFieldManager.LoadFromNBT(nbt, this);
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
				NBTFieldManager.LoadFromNBT(nbt, this);
			}
		}

		[NBT("LevelName")]
		public string worldName = "MCUtils generated world " + new Random().Next(10000);
		[NBT("DataVersion")]
		public int dataVersion;
		[NBT("LastPlayed")]
		public long lastPlayedUnixTimestamp;
		[NBT("WasModded")]
		public bool wasModded;

		public Player player = new Player(new Vector3(0,0,0));

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
			var levelNBT = levelDat.contents;
			var d = new LevelData();
			NBTFieldManager.LoadFromNBT(levelNBT, d);
			if (levelNBT.TryGet<NBTCompound>("Player", out var playerComp))
			{
				d.player = new Player(playerComp);
			}
			d.spawnpoint = new Spawnpoint(levelNBT);
			d.gameTypeAndDifficulty = new GameTypeAndDifficulty(levelNBT);
			if (levelNBT.TryGet<NBTCompound>("GameRules", out var rulesComp))
			{
				d.gameRules = new GameRules(rulesComp);
			}
			if (levelNBT.TryGet<NBTCompound>("WorldGenSettings", out var wg))
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
using System;
using System.Collections.Generic;
using WorldForge.Biomes;
using WorldForge.NBT;

namespace WorldForge
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

			public NBTCompound CreateNBT()
			{
				var comp = new NBTCompound();
				foreach(var f in typeof(GameRules).GetFields())
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
				NBTConverter.LoadFromNBT(levelDat, this);
			}
		}

		public struct SuperflatLayer
		{
			[NBT("block")]
			public string block;
			[NBT("height")]
			public int height;

			public SuperflatLayer(string block, int height)
			{
				this.block = block;
				this.height = height;
			}

			public NBTCompound ToNBT()
			{
				return NBTCompound.FromObject(this);
			}
		}

		public abstract class BiomeSource : INBTConverter
		{
			[NBT("type")]
			public readonly string type;

			public BiomeSource(string type)
			{
				this.type = type;
			}

			public static BiomeSource CreateFromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				string type = comp.Get<string>("type");
				if(type.StartsWith("minecraft:")) type = type.Replace("minecraft:", "");
				if(type == "checkerboard") return new CheckerboardBiomeSource(comp);
				else if(type == "fixed") return new FixedBiomeSource(comp.Get<string>("biome"));
				else if(type == "multi_noise") return new MultiNoiseBiomeSource(comp);
				else if(type == "the_end") return new TheEndBiomeSource();
				else if(type == "vanilla_layered") return new VanillaLayeredBiomeSource(comp);
				else throw new NotImplementedException("Unknown BiomeSource type: " + type);
			}

			public virtual void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public virtual object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		//Legacy biome source, not used in newer versions (1.16+ ?)
		public class VanillaLayeredBiomeSource : BiomeSource
		{
			[NBT("seed")]
			public long seed;
			[NBT("large_biomes")]
			public bool largeBiomes = false;

			public VanillaLayeredBiomeSource(long seed, bool largeBiomes = false) : base("vanilla_layered")
			{
				this.seed = seed;
				this.largeBiomes = largeBiomes;
			}

			public VanillaLayeredBiomeSource(NBTCompound nbt) : base("vanilla_layered")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				comp.TryGet("seed", out seed);
				comp.TryGet("large_biomes", out largeBiomes);
			}

			public override object ToNBT(GameVersion version)
			{
				var comp = (NBTCompound)base.ToNBT(version);
				comp.Add("seed", seed);
				comp.Add("large_biomes", largeBiomes);
				return comp;
			}
		}

		public class CheckerboardBiomeSource : BiomeSource
		{
			public List<string> biomes = new List<string>();
			public int? scale = null;

			public CheckerboardBiomeSource(string biomeOrTag, int scale = 2) : base("checkerboard")
			{
				biomes.Add(biomeOrTag);
				this.scale = scale;
			}

			public CheckerboardBiomeSource(List<string> biomes, int scale = 2) : base("checkerboard")
			{
				this.biomes = biomes;
				this.scale = scale;
			}

			public CheckerboardBiomeSource(NBTCompound nbt) : base("checkerboard")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				var b = comp.Get("biomes");
				if(b is NBTList nbtList)
				{
					biomes = nbtList.ToList<string>();
				}
				else
				{
					biomes = new List<string>() { (string)b };
				}
				if(comp.TryGet("scale", out int s)) scale = s;
			}

			public override object ToNBT(GameVersion version)
			{
				return base.ToNBT(version);
			}
		}

		public class FixedBiomeSource : BiomeSource
		{
			[NBT("biome")]
			public string biome;

			public FixedBiomeSource(string biome) : base("fixed")
			{
				this.biome = biome;
			}
		}

		public class MultiNoiseBiomeSource : BiomeSource
		{
			public string preset;
			public NBTList customBiomes;
			public long seed;

			public MultiNoiseBiomeSource(string preset) : base("multi_noise")
			{
				this.preset = preset;
			}

			public MultiNoiseBiomeSource(NBTList biomes) : base("multi_noise")
			{
				customBiomes = biomes;
			}

			public MultiNoiseBiomeSource(NBTCompound nbt) : base("multi_noise")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				if(comp.TryGet("preset", out string p)) preset = p;
				else customBiomes = comp.Get<NBTList>("biomes");
				comp.TryGet("seed", out seed);
			}

			public override object ToNBT(GameVersion version)
			{
				var comp = (NBTCompound)base.ToNBT(version);
				if(customBiomes != null) comp.Add("biomes", customBiomes);
				else comp.Add("preset", preset);
				comp.Add("seed", seed);
				return comp;
			}
		}

		public class TheEndBiomeSource : BiomeSource
		{
			public TheEndBiomeSource() : base("the_end")
			{

			}
		}

		public class DimensionGenerator : INBTConverter
		{
			public string dimType;
			public string genType;

			public string genSettingsID;
			public NBTCompound customGenSettings;

			public long? seed;

			public BiomeID? singleBiomeSource = null;
			public BiomeSource biomeSource;

			public readonly bool isSuperflat = false;
			public SuperflatLayer[] superflatLayers;
			public BiomeID superflatBiome = BiomeID.the_void;
			public bool superflatFeatures = false;
			public bool superflatLakes = false;

			private DimensionGenerator(string dimType, string genType, string genSetting, long? seed)
			{
				this.dimType = dimType;
				this.genType = genType;
				this.genSettingsID = genSetting;
				this.seed = seed;
				isSuperflat = genType == "minecraft:flat";
			}

			public static DimensionGenerator CreateDefaultOverworldGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator("minecraft:overworld", "minecraft:noise", "minecraft:overworld", customSeed);
				return gen;
			}

			public static DimensionGenerator CreateSuperflatOverworldGenerator(BiomeID biome, params SuperflatLayer[] layers)
			{
				var gen = new DimensionGenerator("minecraft:overworld", "minecraft:flat", null, null);
				gen.superflatBiome = biome;
				gen.superflatLayers = layers;
				return gen;
			}

			public static DimensionGenerator CreateDefaultNetherGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator("minecraft:the_nether", "minecraft:noise", "minecraft:nether", customSeed);
				return gen;
			}

			public static DimensionGenerator CreateDefaultEndGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator("minecraft:the_end", "minecraft:noise", "minecraft:end", customSeed);
				gen.singleBiomeSource = BiomeID.the_end;
				return gen;
			}

			public object ToNBT(GameVersion version)
			{
				var comp = new NBTCompound();
				comp.Add("type", dimType);
				var gen = comp.AddCompound("generator");
				gen.Add("type", genType);
				if(seed.HasValue) gen.Add("seed", seed);
				if(isSuperflat)
				{
					var settings = gen.AddCompound("settings");
					if(superflatLayers != null)
					{
						var list = settings.AddList("layers", NBTTag.TAG_Compound);
						for(int i = 0; i < superflatLayers.Length; i++)
						{
							list.Add(superflatLayers[i].ToNBT());
						}
					}
					//Not sure if this should be a string list
					settings.AddList("structure_overrides", NBTTag.TAG_String);
					settings.Add("biome", superflatBiome.ToString());
					settings.Add("features", superflatFeatures);
					settings.Add("lakes", superflatLakes);
				}
				else
				{
					gen.Add("settings", genSettingsID);
					var biomeSource = gen.AddCompound("biome_source");
					if(!singleBiomeSource.HasValue)
					{
						biomeSource.Add("preset", genSettingsID);
						biomeSource.Add("type", "minecraft:multi_noise");
					}
					else
					{
						biomeSource.Add("type", singleBiomeSource.Value.ToString());
					}
				}
				return comp;
			}

			public static DimensionGenerator CreateFromNBT(object nbtData)
			{
				var gen = new DimensionGenerator(null, null, null, null);
				gen.FromNBT(nbtData);
				return gen;
			}

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				dimType = comp.Get<string>("type");
				var genComp = comp.Get<NBTCompound>("generator");
				genType = genComp.Get<string>("type");
				genSettingsID = genComp.Get<string>("settings");
				genComp.TryGet("seed", out seed);
				if(genComp.TryGet<NBTCompound>("biome_source", out var biomeSourceComp))
				{
					biomeSource = BiomeSource.CreateFromNBT(genComp.Get<NBTCompound>("biome_source"));
				}
			}
		}

		public class WorldGenerator : INBTConverter
		{
			public const string overworldID = "minecraft:overworld";
			public const string theNetherID = "minecraft:the_nether";
			public const string theEndID = "minecraft:the_end";

			[NBT("bonus_chest")]
			public bool generateBonusChest = false;
			[NBT("generate_features")]
			public bool generateFeatures = true;
			[NBT("seed")]
			private long seed;
			//[NBT("dimensions")]
			public Dictionary<string, DimensionGenerator> dimensionGenerators;

			public DimensionGenerator OverworldGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(overworldID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(overworldID)) dimensionGenerators[overworldID] = value;
					else dimensionGenerators.Add(overworldID, value);
				}
			}

			public DimensionGenerator NetherGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(theNetherID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(theNetherID)) dimensionGenerators[theNetherID] = value;
					else dimensionGenerators.Add(theNetherID, value);
				}
			}

			public DimensionGenerator EndGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(theEndID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(theEndID)) dimensionGenerators[theEndID] = value;
					else dimensionGenerators.Add(theEndID, value);
				}
			}

			public long WorldSeed => seed;

			public WorldGenerator(long seed)
			{
				this.seed = seed;
				dimensionGenerators = new Dictionary<string, DimensionGenerator>();
				dimensionGenerators.Add(overworldID, DimensionGenerator.CreateDefaultOverworldGenerator());
				dimensionGenerators.Add(theNetherID, DimensionGenerator.CreateDefaultNetherGenerator());
				dimensionGenerators.Add(theEndID, DimensionGenerator.CreateDefaultEndGenerator());
			}

			public WorldGenerator() : this(new Random().Next(int.MinValue, int.MaxValue))
			{

			}

			public WorldGenerator(NBTCompound nbt)
			{
				FromNBT(nbt);
			}

			public void SetWorldSeed(long newSeed)
			{
				seed = newSeed;
			}

			public object ToNBT(GameVersion version)
			{
				NBTCompound comp = new NBTCompound();
				NBTConverter.WriteToNBT(this, comp, version);
				var dim = comp.AddCompound("dimensions");
				foreach(var kv in dimensionGenerators)
				{
					dim.Add(kv.Key, kv.Value.ToNBT(version));
				}
				return comp;
			}

			public void FromNBT(object nbtData)
			{
				var nbt = (NBTCompound)nbtData;
				NBTConverter.LoadFromNBT(nbt, this);
				dimensionGenerators = new Dictionary<string, DimensionGenerator>();
				if(nbt.TryGet("dimensions", out NBTCompound dimensions))
				{
					foreach(var kv in dimensions)
					{
						dimensionGenerators.Add(kv.Key, DimensionGenerator.CreateFromNBT(kv.Value));
					}
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
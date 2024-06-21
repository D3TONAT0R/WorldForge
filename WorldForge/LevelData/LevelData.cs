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
				spawnY = w.Overworld.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
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

		/// <summary>
		/// Data related to the world border.
		/// </summary>
		public class WorldBorder
		{
			/// <summary>
			/// Center of the world border on the X coordinate.
			/// </summary>
			[NBT("BorderCenterX")]
			public double borderCenterX = 0.0d;
			/// <summary>
			/// Center of the world border on the Z coordinate.
			/// </summary>
			[NBT("BorderCenterZ")]
			public double borderCenterZ = 0.0d;
			/// <summary>
			/// The amount of damage the world border deals to players per second per block.
			/// </summary>
			[NBT("BorderDamagePerBlock")]
			public double borderDamagePerBlock = 0.2d;
			/// <summary>
			/// The radius before the world border starts dealing damage.
			/// </summary>
			[NBT("BorderSafeZone")]
			public double borderSafeZone = 5.0d;
			/// <summary>
			/// Width and length of the world border. Default is 60 million blocks.
			/// </summary>
			[NBT("BorderSize")]
			public double borderSize = 60000000d;
			/// <summary>
			/// The border size target to interpolate towards.
			/// </summary>
			[NBT("BorderSizeLerpTarget")]
			public double borderSizeLerpTarget = 60000000;
			/// <summary>
			/// The time in milliseconds until the border reaches its target size.
			/// </summary>
			[NBT("BorderSizeLerpTime")]
			public long borderSizeLerpTime = 0;
			/// <summary>
			/// Maximum distance away from the border until the border warning overlay appears on the player's screen.
			/// </summary>
			[NBT("BorderWarningBlocks")]
			public double borderWarningBlocks = 5.0d;
			/// <summary>
			/// Time in seconds before the player is warned about the shrinking world border.
			/// </summary>
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
			/// The effective number of ticks elapsed since world creation. May or may not correspond to the time of day.
			/// </summary>
			[NBT("Time")]
			public long worldTime = 0;
			/// <summary>
			/// The time of day in ticks. 0 is sunrise, 6000 is midday, 12000 is sunset, and 18000 is midnight. This value keeps counting past 24000.
			/// </summary>
			//TODO: find version where this was added
			[NBT("DayTime", "1.0.0")]
			public long dayTime = 0;

			/// <summary>
			/// The actual time of the current day in ticks. 0 is sunrise, 6000 is midday, 12000 is sunset, and 18000 is midnight.
			/// </summary>
			public long TimeOfDay => dayTime % 24000;

			/// <summary>
			/// The number of days elapsed through the day time.
			/// </summary>
			public double TimeInDays => dayTime / 24000d;

			/// <summary>
			/// The number of ticks until clear weather ends. (1.8+)
			/// </summary>
			[NBT(null, "1.8")]
			public int clearWeatherTime = 0;
			/// <summary>
			/// If true, it is currently raining.
			/// </summary>
			[NBT]
			public bool raining = false;
			/// <summary>
			/// The number of ticks until the rain ends.
			/// </summary>
			[NBT]
			public int rainTime = 12000;
			/// <summary>
			/// If true, it is currently thundering.
			/// </summary>
			[NBT]
			public bool thundering = false;
			/// <summary>
			/// The number of ticks until the thunderstorm ends.
			/// </summary>
			[NBT]
			public int thunderTime = 120000;

			public TimeAndWeather()
			{

			}

			public TimeAndWeather(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			public void SetDayTime(int day, int timeOfDay)
			{
				dayTime = day * 24000 + timeOfDay;
			}

			public void SetTimeSunrise()
			{
				if(TimeOfDay == 0) return;
				dayTime += 24000 - TimeOfDay;
			}

			public void SetTimeMidday()
			{
				if(TimeOfDay == 6000) return;
				SetTimeSunrise();
				dayTime += 6000;
			}

			public void SetTimeSunset()
			{
				if(TimeOfDay == 12000) return;
				SetTimeSunrise();
				dayTime += 12000;
			}

			public void SetTimeMidnight()
			{
				if(TimeOfDay == 18000) return;
				SetTimeSunrise();
				dayTime += 18000;
			}

			public void SetClearWeather(int? fixedDuration = null)
			{
				int duration = fixedDuration ?? random.Next(12000, 180000);
				clearWeatherTime = duration;
				raining = false;
				thundering = false;
				rainTime = duration;
			}

			public void SetRain(int? fixedDuration = null)
			{
				int duration = fixedDuration ?? random.Next(12000, 24000);
				thunderTime = 0;
				raining = true;
				thunderTime = duration;
			}

			public void SetThunderstorm(int? fixedDuration = null)
			{
				SetRain(fixedDuration);
				thundering = true;
				thunderTime = rainTime;
			}
		}

		/// <summary>
		/// Data related to the wandering trader.
		/// </summary>
		public class WanderingTraderInfo
		{
			/// <summary>
			/// The current chance of the wandering trader spawning next attempt; 
			/// this value is the percentage and is divided by 10 when loaded by the game, for example a value of 50 means 5.0% chance.
			/// </summary>
			[NBT("WanderingTraderSpawnChance")]
			public int spawnChance = 25;
			/// <summary>
			/// The amount of ticks until another wandering trader spawn attempt is made.
			/// </summary>
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

		/// <summary>
		/// List of data packs enabled and disabled in the world.
		/// </summary>
		public class DataPacks
		{
			[NBT("Disabled")]
			public List<string> disabled = new List<string>();
			[NBT("Enabled")]
			public List<string> enabled = new List<string>();

			public DataPacks()
			{
				enabled.Add("vanilla");
			}

			public DataPacks(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}
		}

		/// <summary>
		/// Data defining a custom bossbar.
		/// </summary>
		public class CustomBossEvent : INBTConverter
		{
			/// <summary>
			/// A list of players that can see the bossbar.
			/// </summary>
			[NBT("Players")]
			public List<int[]> playerUUIDs = new List<int[]>();
			/// <summary>
			/// ID of the color of the bossbar. Also sets the color of the display text of the bossbar, provided that the display text does not explicitly define a color for itself.
			/// </summary>
			[NBT("Color")]
			public string color = "white";
			/// <summary>
			/// If true, the bossbar creates fog.
			/// </summary>
			[NBT("CreateWorldFog")]
			public bool createWorldFog = false;
			/// <summary>
			/// If true, the bossbar darkens the sky.
			/// </summary>
			[NBT("DarkenScreen")]
			public bool darkenScreen = false;
			/// <summary>
			/// The maximum value of the bossbar.
			/// </summary>
			[NBT("Max")]
			public int maxValue = 100;
			/// <summary>
			/// The current value of the bossbar.
			/// </summary>
			[NBT("Value")]
			public int value = 100;
			/// <summary>
			/// The display name of the bossbar as a JSON text component.
			/// </summary>
			[NBT("Name")]
			public string nameJson = "";
			/// <summary>
			/// The ID of the overlay to be shown over the health bar. Accepted values are: progress, notched_6, notched_10, notched_12, and notched_20.
			/// </summary>
			[NBT("Overlay")]
			public string overlayType = "progress";
			/// <summary>
			/// If true, the bossbar should initiate boss music.
			/// </summary>
			[NBT("PlayBossMusic")]
			public bool playBossMusic = false;
			/// <summary>
			/// If true, the bossbar will be visible to the listed players.
			/// </summary>
			[NBT("Visible")]
			public bool visible = true;

			public static Dictionary<string, CustomBossEvent> LoadEventsFromNBT(NBTCompound dat)
			{
				var dict = new Dictionary<string, CustomBossEvent>();
				if(dat.TryGet("CustomBossEvents", out NBTCompound collection))
				{
					foreach(var key in collection.GetContentKeys())
					{
						var bossEvent = new CustomBossEvent(collection.GetAsCompound(key));
						dict.Add(key, bossEvent);
					}
				}
				return dict;
			}

			public CustomBossEvent()
			{

			}

			public CustomBossEvent(NBTCompound nbt)
			{
				FromNBT(nbt);
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		public class DragonFight : INBTConverter
		{
			[NBT("DragonKilled")]
			public bool dragonKilled = false;
			[NBT("NeedsStateScanning")]
			public bool needsStateScanning = true;
			[NBT("PreviouslyKilled")]
			public bool previouslyKilled = false;
			[NBT("Gateways")]
			public List<int> gateways = new List<int>() { 18, 8, 10, 13, 14, 5, 15, 1, 0, 7, 11, 17, 3, 19, 6, 2, 9, 12, 4, 16 };

			public DragonFight()
			{

			}

			public DragonFight(NBTCompound nbt)
			{
				if(nbt.TryGet<NBTCompound>("DragonFight", out var comp))
				{
					FromNBT(comp);
				}
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		[NBT("DataVersion")]
		public int dataVersion;

		//TODO: find out when this was added
		/// <summary>
		/// Normally true after a world has been initialized properly after creation. 
		/// If the initial simulation was canceled somehow, this can be false and the world is re-initialized on next load.
		/// </summary>
		[NBT("initialized")]
		public bool initialized = true;
		/// <summary>
		/// The name of the world.
		/// </summary>
		[NBT("LevelName")]
		public string worldName = "MCUtils generated world " + new Random().Next(10000);
		/// <summary>
		/// The time the world was last played, in Unix timestamp format.
		/// </summary>
		[NBT("LastPlayed")]
		public long lastPlayedUnixTimestamp;
		//TODO: find out when this was added
		/// <summary>
		/// List of experimental features enabled for this world. Defaults to null if there are no expirimental features enabled. (1.19+)
		/// </summary>
		[NBT("enabled_features", "1.19")]
		public List<string> enabledFeatures;
		//TODO: find out when this was added
		/// <summary>
		/// True if the world was opened in a modified version. (1.13+)
		/// </summary>
		[NBT("WasModded", "1.13.0")]
		public bool wasModded;

		/// <summary>
		/// Data related to the (single player) player.
		/// </summary>
		public Player player = new Player(new Vector3(0, 0, 0));

		/// <summary>
		/// The world spawnpoint where players will spawn.
		/// </summary>
		public Spawnpoint spawnpoint = new Spawnpoint();

		/// <summary>
		/// Settings related to game type and difficulty.
		/// </summary>
		public GameTypeAndDifficulty gameTypeAndDifficulty = new GameTypeAndDifficulty();

		/// <summary>
		/// Settings related to time and weather.
		/// </summary>
		public TimeAndWeather timeAndWeather = new TimeAndWeather();

		/// <summary>
		/// The game rules set for the world.
		/// </summary>
		public GameRules gameRules = new GameRules();

		/// <summary>
		/// Settings related to world generation.
		/// </summary>
		public WorldGenerator worldGen = new WorldGenerator();

		/// <summary>
		/// Settings related to the world border.
		/// </summary>
		public WorldBorder worldBorder = new WorldBorder();

		/// <summary>
		/// Settings related to the wandering trader.
		/// </summary>
		public WanderingTraderInfo wanderingTraderInfo = new WanderingTraderInfo();

		/// <summary>
		/// List of data packs enabled and disabled in the world.
		/// </summary>
		public DataPacks dataPacks = new DataPacks();

		/// <summary>
		/// List of custom boss events in the world.
		/// </summary>
		public Dictionary<string, CustomBossEvent> customBossEvents = new Dictionary<string, CustomBossEvent>();

		/// <summary>
		/// Data related to the dragon fight.
		/// </summary>
		public DragonFight dragonFight = new DragonFight();

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
			d.customBossEvents = CustomBossEvent.LoadEventsFromNBT(levelNBT);
			d.dragonFight = new DragonFight(levelNBT);
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
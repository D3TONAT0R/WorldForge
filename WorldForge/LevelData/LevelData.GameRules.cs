using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public partial class LevelData
	{
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
			[NBT(null, "1.12", "1.13")]
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
	}
}

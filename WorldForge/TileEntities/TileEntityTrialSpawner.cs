using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityTrialSpawner : TileEntity
	{
		public class Configuration : INBTConverter
		{
			[NBT("spawn_range")]
			public int spawnRange = 4;
			[NBT("total_mobs")]
			public float totalMobs = 6f;
			[NBT("simultaneous_mobs")]
			public float simultaneousMobs = 2f;
			[NBT("total_mobs_added_per_player")]
			public float totalMobsAddedPerPlayer = 2f;
			[NBT("simultaneous_mobs_added_per_player")]
			public float simultaneousMobsAddedPerPlayer = 1f;
			[NBT("ticks_between_spawns")]
			public int ticksBetweenSpawns = 40;
			[NBT("spawn_potentials")]
			public List<NBTCompound> spawnPotentials = new List<NBTCompound>();
			[NBT("loot_tables_to_eject")]
			public List<NBTCompound> lootTablesToEject = new List<NBTCompound>();
			[NBT("items_to_drop_when_ominous")]
			public string itemsToDropWhenOminous = "minecraft:spawners/trial_chamber/items_to_drop_when_ominous";

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		[NBT("required_player_range")]
		public int requiredPlayerRange = 14;
		[NBT("target_cooldown_length")]
		public int targetCooldownLength = 36000;
		[NBT("normal_config")]
		public Configuration normalConfig = null;
		[NBT("ominous_config")]
		public Configuration ominousConfig = null;

		[NBT("registered_players")]
		public List<UUID> registeredPlayers = new List<UUID>();
		[NBT("current_mobs")]
		public List<UUID> currentMobs = new List<UUID>();

		[NBT("cooldown_ends_at")]
		public long cooldownEndsAt = 0;
		[NBT("next_mob_spawns_at")]
		public long nextMobSpawnsAt = 0;
		[NBT("total_mobs_spawned")]
		public int totalMobsSpawned = 0;
		[NBT("spawn_data")]
		public NBTCompound spawnData;

		[NBT("ejecting_loot_table")]
		public string ejectingLootTable = null;

		public override GameVersion AddedInVersion => GameVersion.Release_1(21);

		public TileEntityTrialSpawner() : base("trial_spawner")
		{

		}

		public TileEntityTrialSpawner(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

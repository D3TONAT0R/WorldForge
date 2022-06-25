using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class LevelDATSerializer
	{

		public static LevelDATSerializer CreateForVersion(Version gameVersion)
		{
			//TODO: implement different dat serializers for newer versions
			return new LevelDATSerializer();
		}

		private LevelDATSerializer() { }

		public virtual void WriteLevelDAT(World world, NBTContent nbt, bool creativeMode)
		{
			var dat = nbt.contents.AddCompound("Data");

			var dv = world.gameVersion.GetDataVersion();
			if(dv != null) dat.Add("DataVersion", dv.Value);

			dat.Add("version", world.gameVersion >= Version.FirstAnvilVersion ? 19133 : 19132);

			WriteWorldInfo(world, dat);
			WriteSpawnPoint(dat, world.worldSpawnX, world.worldSpawnY, world.worldSpawnZ);
			WritePlayerData(world, dat);

			//TODO: make separate DAT serializer for "newer" NBT data
			if(world.gameVersion >= Version.Beta_1(8))
			{
				dat.Add("GameType", creativeMode ? 1 : 0);
			}
			//TODO: find out when cheat option was added
			if(world.gameVersion >= Version.Release_1(0))
			{
				dat.Add("allowCommands", (byte)(creativeMode ? 1 : 0));
			}
		}

		protected virtual void WriteWorldInfo(World world, CompoundContainer dat)
		{
			dat.Add("LevelName", world.worldName);
			dat.Add("LastPlayed", 0L);
			dat.Add("raining", (byte)0);
			dat.Add("rainTime", 0);
			dat.Add("thundering", (byte)0);
			dat.Add("Time", 0L);
			dat.Add("RandomSeed", world.worldSeed);
		}

		protected virtual void WriteSpawnPoint(CompoundContainer dat, int spawnX, int spawnY, int spawnZ)
		{
			dat.Add("SpawnX", spawnX);
			dat.Add("SpawnY", spawnY);
			dat.Add("SpawnZ", spawnZ);
		}

		protected void WritePlayerData(World world, CompoundContainer dat)
		{
			CompoundContainer player = new CompoundContainer();
			dat.Add("Player", player);
			player.Add("Inventory", new ListContainer(NBTTag.TAG_Compound));
			var pos = player.AddList("Pos", NBTTag.TAG_Double);
			pos.Add(world.worldSpawnX + 0.5d);
			pos.Add(world.worldSpawnY + 1.0d);
			pos.Add(world.worldSpawnZ + 0.5d);
			var rot = player.AddList("Rotation", NBTTag.TAG_Double);
			rot.Add(0d);
			rot.Add(0d);
			var motion = player.AddList("Motion", NBTTag.TAG_Double);
			motion.Add(0d);
			motion.Add(0d);
			motion.Add(0d);
			player.Add("Air", (short)300);
			player.Add("AttackTime", (short)0);
			player.Add("DeathTime", (short)0);
			player.Add("Dimension", 0);
			player.Add("FallDistance", 0f);
			player.Add("Fire", (short)-20);
			player.Add("Health", (short)20);
			player.Add("HurtTime", (short)0);
			player.Add("OnGround", (byte)1);
			player.Add("Score", 0);
			player.Add("Sleeping", (byte)0);
			player.Add("SleepTimer", (short)0);
		}

		/*
		
		private NBTContent CreateLevelDAT(int playerPosX, int playerPosY, int playerPosZ, bool creativeModeWithCheats)
		{

			NBTContent levelDAT = new NBTContent();
			var data = levelDAT.contents.AddCompound("Data");

			data.Add<int>("DataVersion", 2504);
			data.Add<byte>("initialized", 0);
			data.Add<long>("LastPlayed", 0);
			data.Add<byte>("WasModded", 0);

			var datapacks = data.AddCompound("DataPacks");
			datapacks.Add("Disabled", new ListContainer(NBTTag.TAG_String));
			datapacks.Add("Enabled", new ListContainer(NBTTag.TAG_String)).Add(null, "vanilla");

			data.AddCompound("GameRules");

			data.Add("Player", CreatePlayerCompound(playerPosX, playerPosY, playerPosZ, creativeModeWithCheats));

			var versionComp = data.AddCompound("Version");
			versionComp.Add<int>("Id", 2504);
			versionComp.Add<string>("Name", "1.16");
			versionComp.Add<byte>("Snapshot", 0);

			var worldGenComp = data.AddCompound("WorldGenSettings");
			worldGenComp.AddCompound("dimensions");
			worldGenComp.Add<byte>("bonus_chest", 0);
			worldGenComp.Add<byte>("generate_features", 1);
			worldGenComp.Add<long>("seed", new Random().Next(int.MaxValue));

			data.AddList("ScheduledEvents", NBTTag.TAG_List);
			data.AddList("ServerBrands", NBTTag.TAG_String).Add("vanilla");
			data.Add<byte>("allowCommands", (byte)(creativeModeWithCheats ? 1 : 0));

			data.Add<double>("BorderCenterX", 0);
			data.Add<double>("BorderCenterZ", 0);
			data.Add<double>("BorderDamagePerBlock", 0.2d);
			data.Add<double>("BorderSafeZone", 5);
			data.Add<double>("BorderSize", 60000000);
			data.Add<double>("BorderSizeLerpTarget", 60000000);
			data.Add<long>("BorderSizeLerpTime", 0);
			data.Add<double>("BorderWarningBlocks", 5);
			data.Add<double>("BorderWarningTime", 15);

			data.Add<int>("clearWeatherTime", 0);
			data.Add<long>("DayTime", 0);
			data.Add<byte>("raining", 0);
			data.Add<int>("rainTime", new Random().Next(20000, 200000));
			data.Add<byte>("thundering", 0);
			data.Add<int>("thunderTime", new Random().Next(50000, 100000));
			data.Add<long>("Time", 0);
			data.Add<int>("version", 19133);

			data.Add<byte>("Difficulty", 2);
			data.Add<byte>("DifficultyLocked", 0);

			data.Add<int>("GameType", creativeModeWithCheats ? 1 : 0);
			data.Add<byte>("hardcore", 0);

			data.Add<string>("LevelName", worldName);

			data.Add<float>("SpawnAngle", 0);
			data.Add<int>("SpawnX", playerPosX);
			data.Add<int>("SpawnY", playerPosY);
			data.Add<int>("SpawnZ", playerPosZ);

			data.Add<int>("WanderingTraderSpawnChance", 50);
			data.Add<int>("WanderingTraderSpawnDelay", 24000);

			return levelDAT;
		}

		private CompoundContainer CreatePlayerCompound(int posX, int posY, int posZ, bool creativeModeWithCheats)
		{
			var player = new CompoundContainer();

			var abilities = player.AddCompound("abilities");
			abilities.Add<byte>("flying", 0);
			abilities.Add<float>("flySpeed", 0.05f);
			abilities.Add<byte>("instabuild", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<byte>("invulnerable", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<byte>("mayBuild", 0);
			abilities.Add<byte>("mayfly", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<float>("walkSpeed", 0.1f);

			player.AddCompound("Brain").AddCompound("memories");
			player.AddCompound("recipeBook");
			player.Add("Attributes", new ListContainer(NBTTag.TAG_Compound));
			player.Add("EnderItems", new ListContainer(NBTTag.TAG_Compound));
			player.Add("Inventory", new ListContainer(NBTTag.TAG_Compound));
			player.AddList("Motion", NBTTag.TAG_Double).AddRange(0d, 0d, 0d);

			var pos = player.AddList("Pos", NBTTag.TAG_Double);
			pos.Add<double>(posX);
			pos.Add<double>(posY);
			pos.Add<double>(posZ);
			player.AddList("Rotation", NBTTag.TAG_Float).AddRange(0f, 0f);

			player.Add("AbsorptionAmount", 0f);
			player.Add<short>("Air", 300);
			player.Add<short>("DeathTime", 0);
			player.Add<string>("Dimension", "minecraft:overworld");
			player.Add<float>("FallDistance", 0);
			player.Add<byte>("FallFlying", 0);
			player.Add<short>("Fire", -20);
			player.Add<float>("foodExhaustionLevel", 0);
			player.Add<int>("foodLevel", 20);
			player.Add<float>("foodSaturationLevel", 5);
			player.Add<int>("foodTickTimer", 0);
			player.Add<float>("Health", 20);
			player.Add<int>("HurtByTimestamp", 0);
			player.Add<short>("HurtTime", 0);
			player.Add<byte>("Invulnerable", 0);
			player.Add<byte>("OnGround", 0);
			player.Add<int>("playerGameType", creativeModeWithCheats ? 1 : 0);
			player.Add<int>("Score", 0);
			player.Add<byte>("seenCredits", 0);
			player.Add<int>("SelectedItemSlot", 0);
			player.Add<short>("SleepTimer", 0);
			player.Add<int>("XpLevel", 0);
			player.Add<float>("XpP", 0);
			player.Add<int>("XpSeed", 0);
			player.Add<int>("XpTotal", 0);

			player.Add<int>("DataVersion", 2504);

			//UUID?
			player.Add<int[]>("UUID", new int[] { 0, 0, 0, 0 });

			return player;
		}
		*/
	}
}

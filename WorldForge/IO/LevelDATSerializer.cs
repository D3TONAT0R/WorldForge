using WorldForge.NBT;

namespace WorldForge.IO
{
	public class LevelDATSerializer
	{

		public static LevelDATSerializer CreateForVersion(GameVersion gameVersion)
		{
			//TODO: implement different dat serializers for newer versions
			return new LevelDATSerializer();
		}

		private LevelDATSerializer() { }

		public virtual void WriteLevelDAT(World world, NBTFile levelDatNBT, bool creativeMode)
		{
			var nbt = levelDatNBT.contents.AddCompound("Data");

			var dat = world.levelData;
			var version = world.gameVersion;

			nbt.Add("version", LevelData.GetNBTVersion(version));
			if(version.GetDataVersion().HasValue)
			{
				nbt.Add("DataVersion", version.GetDataVersion().Value);

				var versionInfoNBT = nbt.AddCompound("Version");
				versionInfoNBT.Add("Id", version.GetDataVersion().Value);
				versionInfoNBT.Add("Name", version.ToString());
				versionInfoNBT.Add("Snapshot", false);
				if(version >= GameVersion.Release_1(18))
				{
					versionInfoNBT.Add("Series", "main");
				}
			}

			WriteWorldInfo(dat, nbt, world.gameVersion);
			WriteSpawnPoint(world, dat.spawnpoint, nbt);
			WritePlayerData(world, dat.player, nbt, version);
			WriteTimeAndWeather(world, dat.timeAndWeather, nbt, version);
			WriteGameRules(nbt, dat.gameRules, version);
			WriteWorldGenAndSeed(nbt, dat.worldGen, version);
			WriteWorldBorder(nbt, dat.worldBorder, version);
			WriteWanderingTraderInfo(nbt, dat.wanderingTraderInfo, version);
			WriteDataPackInfo(nbt, dat.dataPacks, version);

			//TODO: make separate DAT serializer for "newer" NBT data
			if(world.gameVersion >= GameVersion.Beta_1(8))
			{
				nbt.Add("GameType", creativeMode ? 1 : 0);
			}
			if(world.gameVersion >= GameVersion.Release_1(3, 1))
			{
				nbt.Add("allowCommands", (byte)(creativeMode ? 1 : 0));
			}
		}

		private void WriteDataPackInfo(NBTCompound nbt, LevelData.DataPacks dataPacks, GameVersion version)
		{
			if(version >= GameVersion.Release_1(13))
			{
				var comp = nbt.AddCompound("DataPacks");
				NBTConverter.WriteToNBT(dataPacks, comp, version);
			}
		}

		private void WriteWanderingTraderInfo(NBTCompound nbt, LevelData.WanderingTraderInfo wanderingTraderInfo, GameVersion version)
		{
			if(version >= GameVersion.Release_1(14))
			{
				NBTConverter.WriteToNBT(wanderingTraderInfo, nbt, version);
			}
		}

		private void WriteWorldBorder(NBTCompound nbt, LevelData.WorldBorder worldBorder, GameVersion version)
		{
			if(version >= GameVersion.Release_1(8))
			{
				NBTConverter.WriteToNBT(worldBorder, nbt, version);
			}
		}

		private void WriteWorldGenAndSeed(NBTCompound nbt, LevelData.WorldGenerator worldGen, GameVersion gameVersion)
		{
			worldGen.WriteToNBT(nbt, gameVersion);
		}

		private void WriteGameRules(NBTCompound nbt, LevelData.GameRules gameRules, GameVersion gameVersion)
		{
			if(gameVersion > GameVersion.Release_1(4, 2))
			{
				nbt.Add("GameRules", gameRules.CreateNBT(gameVersion));
			}
		}

		protected virtual void WriteWorldInfo(LevelData dat, NBTCompound nbt, GameVersion targetVersion)
		{
			nbt.Add("LevelName", dat.worldName);
			nbt.Add("LastPlayed", dat.lastPlayedUnixTimestamp);
			if(targetVersion >= GameVersion.Release_1(0, 0))
			nbt.Add("WasModded", dat.wasModded);
			//TODO: calculate size on disk
			nbt.Add("SizeOnDisk", (long)0);
		}

		protected void WritePlayerData(World world, Player player, NBTCompound dat, GameVersion version)
		{
			if(player.position.IsZero)
			{
				player.position = new Vector3(
					world.levelData.spawnpoint.spawnX + 0.5d,
					world.levelData.spawnpoint.spawnY + 1.0d,
					world.levelData.spawnpoint.spawnZ + 0.5d
				);
			}
			dat.Add("Player", player.ToNBT(version));
		}

		protected void WriteTimeAndWeather(World world, LevelData.TimeAndWeather timeAndWeather, NBTCompound dat, GameVersion version)
		{
			NBTConverter.WriteToNBT(timeAndWeather, dat, version);
		}

		protected virtual void WriteSpawnPoint(World w, LevelData.Spawnpoint s, NBTCompound nbt)
		{
			NBTConverter.WriteToNBT(s, nbt, w.gameVersion);
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

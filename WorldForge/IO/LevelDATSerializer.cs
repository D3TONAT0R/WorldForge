using System.Collections.Generic;
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

		public virtual NBTFile CreateNBTFile(World world)
		{
			var file = new NBTFile();
			var nbt = file.contents.AddCompound("Data");

			var dat = world.LevelData;
			var version = world.GameVersion;

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
			nbt.Add("initialized", dat.initialized);

			//TODO: make separate DAT serializer for "newer" NBT data
			WriteWorldInfo(dat, nbt, world.GameVersion);
			WriteSpawnPoint(world, dat.spawnpoint, nbt);
			WriteGameTypeAndDifficulty(dat.gameTypeAndDifficulty, nbt, version);
			WritePlayerData(world, dat.player, nbt, version);
			WriteTimeAndWeather(world, dat.timeAndWeather, nbt, version);
			WriteGameRules(nbt, dat.gameRules, version);
			WriteWorldGenAndSeed(nbt, dat.worldGen, version);
			WriteWorldBorder(nbt, dat.worldBorder, version);
			WriteWanderingTraderInfo(nbt, dat.wanderingTraderInfo, version);
			WriteCustomBossEvents(nbt, dat.customBossEvents, version);
			WriteDataPackInfo(nbt, dat.dataPacks, version);
			WriteDragonFightInfo(nbt, dat.dragonFight, version);

			return file;
		}

		protected virtual void WriteGameTypeAndDifficulty(LevelData.GameTypeAndDifficulty gameTypeAndDifficulty, NBTCompound nbt, GameVersion version)
		{
			gameTypeAndDifficulty.WriteToNBT(nbt, version);
		}

		protected virtual void WriteDataPackInfo(NBTCompound nbt, LevelData.DataPacks dataPacks, GameVersion version)
		{
			if(version >= GameVersion.Release_1(13))
			{
				var comp = nbt.AddCompound("DataPacks");
				NBTConverter.WriteToNBT(dataPacks, comp, version);
			}
		}

		protected virtual void WriteDragonFightInfo(NBTCompound nbt, LevelData.DragonFight dragonFight, GameVersion version)
		{
			//TODO: check version
			if(version >= GameVersion.Release_1(0))
			{
				nbt.Add("DragonFight", dragonFight.ToNBT(version));
			}
		}

		protected virtual void WriteWanderingTraderInfo(NBTCompound nbt, LevelData.WanderingTraderInfo wanderingTraderInfo, GameVersion version)
		{
			if(version >= GameVersion.Release_1(14))
			{
				NBTConverter.WriteToNBT(wanderingTraderInfo, nbt, version);
			}
		}

		protected virtual void WriteCustomBossEvents(NBTCompound nbt, Dictionary<string, LevelData.CustomBossEvent> customBossEvents, GameVersion version)
		{
			if(version >= GameVersion.Release_1(13))
			{
				var comp = nbt.AddCompound("CustomBossEvents");
				foreach(var kv in customBossEvents)
				{
					comp.Add(kv.Key, kv.Value.ToNBT(version));
				}
			}
		}

		protected virtual void WriteWorldBorder(NBTCompound nbt, LevelData.WorldBorder worldBorder, GameVersion version)
		{
			if(version >= GameVersion.Release_1(8))
			{
				NBTConverter.WriteToNBT(worldBorder, nbt, version);
			}
		}

		protected virtual void WriteWorldGenAndSeed(NBTCompound nbt, LevelData.WorldGenerator worldGen, GameVersion gameVersion)
		{
			worldGen.WriteToNBT(nbt, gameVersion);
		}

		protected virtual void WriteGameRules(NBTCompound nbt, LevelData.GameRules gameRules, GameVersion gameVersion)
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
			if(targetVersion >= GameVersion.Release_1(13, 0)) nbt.Add("WasModded", dat.wasModded);
			//TODO: calculate size on disk
			nbt.Add("SizeOnDisk", (long)0);
		}

		protected virtual void WritePlayerData(World world, Player player, NBTCompound dat, GameVersion version)
		{
			if(player.position.IsZero)
			{
				player.position = new Vector3(
					world.LevelData.spawnpoint.spawnX + 0.5d,
					world.LevelData.spawnpoint.spawnY + 1.0d,
					world.LevelData.spawnpoint.spawnZ + 0.5d
				);
			}
			dat.Add("Player", player.ToNBT(version));
		}

		protected virtual void WriteTimeAndWeather(World world, LevelData.TimeAndWeather timeAndWeather, NBTCompound dat, GameVersion version)
		{
			NBTConverter.WriteToNBT(timeAndWeather, dat, version);
		}

		protected virtual void WriteSpawnPoint(World w, LevelData.Spawnpoint s, NBTCompound nbt)
		{
			NBTConverter.WriteToNBT(s, nbt, w.GameVersion);
		}
	}
}

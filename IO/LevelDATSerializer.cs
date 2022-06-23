using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class LevelDATSerializer
	{
		public virtual void WriteLevelDAT(World world, NBTContent nbt)
		{
			var dat = nbt.contents.AddCompound("Data");

			var dv = world.gameVersion.GetDataVersion();
			if(dv != null) dat.Add("DataVersion", dv.Value);

			WriteWorldInfo(world, dat);
			WriteSpawnPoint(dat, world.worldSpawnX, world.worldSpawnY, world.worldSpawnZ);
			WritePlayerData(world, dat);
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
			dat.Add("version", world.gameVersion >= Version.FirstAnvilVersion ? 19133 : 19132);
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
			pos.Add(world.worldSpawnX + 0.5f);
			pos.Add(world.worldSpawnY + 0.5f);
			pos.Add(world.worldSpawnZ + 0.5f);
			var rot = player.AddList("Rotation", NBTTag.TAG_Double);
			rot.Add(0d);
			rot.Add(0d);
			var motion = player.AddList("Motion", NBTTag.TAG_Double);
			motion.Add(0);
			motion.Add(0);
			motion.Add(0);
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
	}
}

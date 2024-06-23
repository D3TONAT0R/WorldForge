using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class MobSpawnerData : INBTCollection
	{
		[NBT("Delay")]
		public short Delay = -1;
		[NBT("MaxNearbyEntities")]
		public short MaxNearbyEntities = 6;
		[NBT("MinSpanwDelay")]
		public short MinSpanwDelay = 200;
		[NBT("MaxSpawnDelay")]
		public short MaxSpawnDelay = 800;
		[NBT("SpawnCount")]
		public short SpawnCount = 4;
		[NBT("SpawnRange")]
		public short SpawnRange = 4;
		[NBT("RequiredPlayerRange")]
		public short RequiredPlayerRange = 16;

		[NBT("SpawnData")]
		public NBTCompound SpawnData = null;
		[NBT("SpawnPotentials")]
		public List<NBTCompound> SpawnPotentials = new List<NBTCompound>();

		public void LoadFromNBT(NBTCompound nbt, bool remove)
		{
			NBTConverter.LoadFromNBT(nbt, this);
		}

		public void WriteToNBT(NBTCompound nbt, GameVersion version)
		{
			NBTConverter.WriteToNBT(this, nbt, version);
		}
	}
}

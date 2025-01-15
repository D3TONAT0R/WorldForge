using System;
using System.Collections.Generic;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public abstract class ChunkSerializer
	{
		private static readonly List<ChunkSerializer> serializerCache = new List<ChunkSerializer>();

		private static object lockObj = new object();

		public virtual bool SeparateEntitiesData => false;

		public virtual bool SeparatePOIData => false;

		public static ChunkSerializer GetOrCreateSerializer<T>(GameVersion version) where T : ChunkSerializer
		{
			lock(lockObj)
			{
				foreach(var s in serializerCache)
				{
					if(s.GetType() == typeof(T)) return s;
				}
				var newSerializer = (T)Activator.CreateInstance(typeof(T), version);
				serializerCache.Add(newSerializer);
				return newSerializer;
			}
		}

		public static ChunkSerializer GetForVersion(GameVersion gameVersion)
		{
			if(gameVersion >= GameVersion.Release_1(18))
			{
				return GetOrCreateSerializer<ChunkSerializer_1_18>(gameVersion);
			}
			else if(gameVersion >= GameVersion.Release_1(16))
			{
				return GetOrCreateSerializer<ChunkSerializer_1_16>(gameVersion);
			}
			else if(gameVersion >= GameVersion.Release_1(15))
			{
				return GetOrCreateSerializer<ChunkSerializer_1_15>(gameVersion);
			}
			else if(gameVersion >= GameVersion.Release_1(13))
			{
				return GetOrCreateSerializer<ChunkSerializer_1_13>(gameVersion);
			}
			else if(gameVersion >= GameVersion.Release_1(2, 1))
			{
				return GetOrCreateSerializer<ChunkSerializerAnvil>(gameVersion);
			}
			else if(gameVersion >= GameVersion.Beta_1(3))
			{
				return GetOrCreateSerializer<ChunkSerializerBeta>(gameVersion);
			}
			else
			{
				return GetOrCreateSerializer<ChunkSerializerAlpha>(gameVersion);
			}
		}

		public static ChunkSerializer CreateForDataVersion(NBTFile nbt)
		{
			var gv = GameVersion.FromDataVersion(nbt.dataVersion);
			if(!gv.HasValue) throw new ArgumentException("Unable to determine game version from NBT.");
			return GetForVersion(gv.Value);
		}

		public virtual bool AddRootLevelCompound => true;

		public GameVersion TargetVersion { get; private set; }

		public ChunkSerializer(GameVersion version)
		{
			TargetVersion = version;
		}

		public virtual void ReadChunkNBT(ChunkData c, GameVersion? version)
		{
			var chunkNBT = GetRootCompound(c.sourceNBT);

			LoadCommonData(c, chunkNBT, version);
			LoadBlocks(c, chunkNBT, version);
			LoadTileEntities(c, chunkNBT, version);
			LoadTileTicks(c, chunkNBT, version);
			LoadBiomes(c, chunkNBT, version);
			LoadEntities(c, chunkNBT, c.ParentRegion, version);
			PostLoad(c, chunkNBT, version);

			c.RecalculateSectionRange();
		}

		public virtual NBTCompound GetRootCompound(NBTFile chunkNBTData) => chunkNBTData.contents.GetAsCompound("Level");

		public abstract void LoadCommonData(ChunkData c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadBlocks(ChunkData c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadTileEntities(ChunkData c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion, GameVersion? version);

		public abstract void LoadBiomes(ChunkData c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadTileTicks(ChunkData c, NBTCompound chunkNBT, GameVersion? version);

		protected virtual void PostLoad(ChunkData c, NBTCompound chunkNBT, GameVersion? version) { }

		public virtual void CreateChunkNBTs(ChunkData c, out NBTFile mainFile, out NBTFile entitiesFile, out NBTFile poiFile)
		{
			CreateNBTFile(out mainFile, out var mainComp);
			if(SeparateEntitiesData) CreateNBTFile(out entitiesFile, out var entitiesComp);
			else entitiesFile = null;
			if(SeparatePOIData) CreateNBTFile(out poiFile, out var poiComp);
			else poiFile = null;

			WriteCommonData(c, mainComp);
			WriteBlocks(c, mainComp);
			WriteBiomes(c, mainComp);
			WriteTileEntities(c, mainComp);
			WriteTileTicks(c, mainComp);
			if(SeparateEntitiesData) WriteEntities(c, mainComp);

			PostWrite(c, mainComp);
		}
		
		protected void CreateNBTFile(out NBTFile file, out NBTCompound root)
		{
			file = new NBTFile();
			NBTCompound chunkNBT;
			if(AddRootLevelCompound)
			{
				chunkNBT = file.contents.AddCompound("Level");
			}
			else
			{
				chunkNBT = file.contents;
			}
		}

		public abstract void WriteCommonData(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteBlocks(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteTileEntities(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteEntities(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteBiomes(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteTileTicks(ChunkData c, NBTCompound chunkNBT);

		protected virtual void PostWrite(ChunkData c, NBTCompound chunkNBT) { }
	}
}

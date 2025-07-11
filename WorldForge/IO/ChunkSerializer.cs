using System;
using System.Collections.Generic;
using WorldForge.Chunks;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.IO
{
	//TODO: load from separate entities and poi files
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

		public virtual void ReadChunkNBT(Chunk c, ChunkSourceData s, GameVersion? version, ExceptionHandling exceptionHandling)
		{
			ReadMainChunkNBT(c, GetRootCompound(s.main), version, exceptionHandling);
			if(s.entities != null)
			{
				ReadEntitiesChunkNBT(c, s.entities.contents, version);
			}
			if(s.poi != null)
			{
				ReadPOIChunkNBT(c, s.poi.contents, version);
			}
		}

		public virtual void ReadMainChunkNBT(Chunk c, NBTCompound nbt, GameVersion? version, ExceptionHandling exceptionHandling)
		{
			LoadCommonData(c, nbt, version);
			LoadBlocks(c, nbt, version);
			LoadTileEntities(c, nbt, version, exceptionHandling);
			LoadTileTicks(c, nbt, version);
			LoadBiomes(c, nbt, version);
			LoadEntities(c, nbt, c.ParentRegion, version);
			PostLoad(c, nbt, version);
			c.RecalculateSectionRange();
		}

		public virtual void ReadEntitiesChunkNBT(Chunk c, NBTCompound nbt, GameVersion? version)
		{
			LoadEntities(c, nbt, c.ParentRegion, version);
		}

		public virtual void ReadPOIChunkNBT(Chunk c, NBTCompound nbt, GameVersion? version)
		{
			LoadPOIs(c, nbt, version);
		}

		public virtual NBTCompound GetRootCompound(NBTFile chunkNBTData) => chunkNBTData.contents.GetAsCompound("Level");

		public abstract void LoadCommonData(Chunk c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadBlocks(Chunk c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadTileEntities(Chunk c, NBTCompound chunkNBT, GameVersion? version, ExceptionHandling exceptionHandling);

		public abstract void LoadEntities(Chunk c, NBTCompound chunkNBT, Region parentRegion, GameVersion? version);

		public abstract void LoadBiomes(Chunk c, NBTCompound chunkNBT, GameVersion? version);

		public abstract void LoadTileTicks(Chunk c, NBTCompound chunkNBT, GameVersion? version);

		public virtual void LoadPOIs(Chunk c, NBTCompound chunkNBT, GameVersion? version) { }

		protected virtual void PostLoad(Chunk c, NBTCompound chunkNBT, GameVersion? version) { }

		public virtual void CreateChunkNBTs(Chunk c, out NBTFile mainFile, out NBTFile entitiesFile, out NBTFile poiFile)
		{
			CreateNBTFile(out mainFile, out var mainComp);
			NBTCompound entitiesComp = null;
			if(SeparateEntitiesData) CreateNBTFile(out entitiesFile, out entitiesComp);
			else entitiesFile = null;
			NBTCompound poiComp = null;
			if(SeparatePOIData) CreateNBTFile(out poiFile, out poiComp);
			else poiFile = null;

			WriteCommonData(c, mainComp);
			WriteBlocks(c, mainComp);
			WriteBiomes(c, mainComp);
			WriteTileEntities(c, mainComp);
			WriteTileTicks(c, mainComp);
			//TODO: check
			WriteEntities(c, entitiesComp ?? mainComp);
			WritePOIs(c, poiComp ?? mainComp);

			PostWrite(c, mainComp);
		}
		
		protected void CreateNBTFile(out NBTFile file, out NBTCompound root)
		{
			file = new NBTFile();
			if(AddRootLevelCompound)
			{
				root = file.contents.AddCompound("Level");
			}
			else
			{
				root = file.contents;
			}
			var dv = TargetVersion.GetDataVersion();
			if(dv.HasValue) file.contents.Add("DataVersion", dv.Value);
		}

		public abstract void WriteCommonData(Chunk c, NBTCompound chunkNBT);

		public abstract void WriteBlocks(Chunk c, NBTCompound chunkNBT);

		public abstract void WriteTileEntities(Chunk c, NBTCompound chunkNBT);

		public abstract void WriteEntities(Chunk c, NBTCompound chunkNBT);

		public abstract void WriteBiomes(Chunk c, NBTCompound chunkNBT);

		public abstract void WriteTileTicks(Chunk c, NBTCompound chunkNBT);

		public virtual void WritePOIs(Chunk c, NBTCompound chunkNBT) { }

		protected virtual void PostWrite(Chunk c, NBTCompound chunkNBT) { }
	}
}

using MCUtils.Coordinates;
using MCUtils.NBT;
using System;

namespace MCUtils.IO
{
	public abstract class ChunkSerializer
	{
		public static ChunkSerializer CreateForVersion(Version gameVersion)
		{
			if(gameVersion >= Version.Release_1(18))
			{
				return new ChunkSerializer_1_18(gameVersion);
			}
			else if(gameVersion >= Version.Release_1(16))
			{
				return new ChunkSerializer_1_16(gameVersion);
			}
			else if (gameVersion >= Version.Release_1(13))
			{
				return new ChunkSerializer_1_13(gameVersion);
			}
			else if(gameVersion >= Version.Release_1(2,1))
			{
				return new ChunkSerializerAnvil(gameVersion);
			}
			else if(gameVersion >= Version.Beta_1(3))
			{
				return new ChunkSerializerMCR(gameVersion);
			}
			else
			{
				throw new NotImplementedException();
				//TODO: add alpha (separate chunk files) format support
			}
		}

		public static ChunkSerializer CreateForDataVersion(NBTData nbt)
		{
			var gv = Version.FromDataVersion(nbt.dataVersion);
			if (!gv.HasValue) throw new ArgumentException("Unable to determine game version from NBT.");
			return CreateForVersion(gv.Value);
		}

		public virtual bool AddRootLevelCompound => true;

		public Version TargetVersion { get; private set; }

		public ChunkSerializer(Version version)
		{
			TargetVersion = version;
		}

		public virtual ChunkData ReadChunkNBT(NBTData chunkNBTData, Region parentRegion, ChunkCoord coords)
		{
			ChunkData c = new ChunkData(parentRegion, chunkNBTData, coords);
			var chunkNBT = chunkNBTData.contents;

			LoadCommonData(c, chunkNBT);
			LoadBlocks(c, chunkNBT);
			LoadTileEntities(c, chunkNBT);
			LoadTileTicks(c, chunkNBT);
			LoadBiomes(c, chunkNBT);
			LoadEntities(c, chunkNBT, parentRegion);
			PostLoad(c, chunkNBT);

			c.RecalculateSectionRange();

			return c;
		}

		public abstract void LoadCommonData(ChunkData c, NBTCompound chunkNBT);

		public abstract void LoadBlocks(ChunkData c, NBTCompound chunkNBT);

		public abstract void LoadTileEntities(ChunkData c, NBTCompound chunkNBT);

		public abstract void LoadEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion);

		public abstract void LoadBiomes(ChunkData c, NBTCompound chunkNBT);

		public abstract void LoadTileTicks(ChunkData c, NBTCompound chunkNBT);

		public virtual NBTData CreateChunkNBT(ChunkData c, Region parentRegion)
		{
			var chunkRootNBT = new NBTData();
			NBTCompound chunkNBT;
			if (AddRootLevelCompound)
			{
				chunkNBT = chunkRootNBT.contents.AddCompound("Level");
			}
			else
			{
				chunkNBT = chunkRootNBT.contents;
			}

			WriteCommonData(c, chunkNBT);
			WriteBlocks(c, chunkNBT);
			WriteBiomes(c, chunkNBT);
			WriteTileEntities(c, chunkNBT);
			WriteTileTicks(c, chunkNBT);
			WriteEntities(c, chunkNBT, parentRegion);
			PostWrite(c, chunkNBT);

			return chunkRootNBT;
		}

		protected virtual void PostLoad(ChunkData c, NBTCompound chunkNBT) { }

		public abstract void WriteCommonData(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteBlocks(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteTileEntities(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion);

		public abstract void WriteBiomes(ChunkData c, NBTCompound chunkNBT);

		public abstract void WriteTileTicks(ChunkData c, NBTCompound chunkNBT);

		protected virtual void PostWrite(ChunkData c, NBTCompound chunkNBT) { }
	}
}

using MCUtils.Coordinates;
using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public abstract class ChunkSerializer
	{
		public static ChunkSerializer CreateForVersion(Version gameVersion)
		{
			//TODO: create different serializers for different versions
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
			else
			{
				//TODO: add "old" region (and alpha) format support
				throw new NotImplementedException();
			}
		}

		public static ChunkSerializer CreateForDataVersion(NBTContent nbt)
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

		public virtual ChunkData ReadChunkNBT(NBTContent chunkNBTData, Region parentRegion, ChunkCoord coords)
		{
			ChunkData c = new ChunkData(parentRegion, chunkNBTData, coords);
			var chunkNBT = chunkNBTData.contents;

			LoadCommonData(c, chunkNBT);
			LoadBlocks(c, chunkNBT);
			LoadTileEntities(c, chunkNBT);
			LoadTileTicks(c, chunkNBT);
			LoadBiomes(c, chunkNBT);
			LoadEntities(c, chunkNBT, parentRegion);

			c.RecalculateSectionRange();

			return c;
		}

		public abstract void LoadCommonData(ChunkData c, CompoundContainer chunkNBT);

		public abstract void LoadBlocks(ChunkData c, CompoundContainer chunkNBT);

		public abstract void LoadTileEntities(ChunkData c, CompoundContainer chunkNBT);

		public abstract void LoadEntities(ChunkData c, CompoundContainer chunkNBT, Region parentRegion);

		public abstract void LoadBiomes(ChunkData c, CompoundContainer chunkNBT);

		public abstract void LoadTileTicks(ChunkData c, CompoundContainer chunkNBT);

		public virtual NBTContent CreateChunkNBT(ChunkData c, Region parentRegion)
		{
			var chunkRootNBT = new NBTContent();
			CompoundContainer chunkNBT;
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

			return chunkRootNBT;
		}

		public abstract void WriteCommonData(ChunkData c, CompoundContainer chunkNBT);

		public abstract void WriteBlocks(ChunkData c, CompoundContainer chunkNBT);

		public abstract void WriteTileEntities(ChunkData c, CompoundContainer chunkNBT);

		public abstract void WriteEntities(ChunkData c, CompoundContainer chunkNBT, Region parentRegion);

		public abstract void WriteBiomes(ChunkData c, CompoundContainer chunkNBT);

		public abstract void WriteTileTicks(ChunkData c, CompoundContainer chunkNBT);

		/*
		///<summary>Reads all blocks from the given chunk NBT</summary>
		public static void ReadBlocksFromNBT(ChunkData chunk, int? dataVersion)
		{
			Version? version = Version.FromDataVersion(dataVersion);
			var nbtCompound = chunk.sourceNBT.contents;
			if (nbtCompound.Contains("Sections") || nbtCompound.Contains("sections"))
			{
				LoadBlocksAnvilFormat(chunk, nbtCompound, version);
			}
			else
			{
				LoadBlocksMCRFormat(chunk, nbtCompound);
			}
		}

		static void LoadBlocksAnvilFormat(ChunkData chunk, CompoundContainer nbtCompound, Version? version)
		{
			//Format changed between 1.17 and 1.18
			if (version < Version.Release_1(18))
			{
				var sectionsList = nbtCompound.GetAsList("Sections");
				foreach (var o in sectionsList.cont)
				{
					var section = new ChunkSection(null);

					var compound = (CompoundContainer)o;
					if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
					sbyte secY;
					unchecked
					{
						//secY = Convert.ToSByte(compound.Get("Y"));
						secY = (sbyte)compound.Get<byte>("Y");
					}
					section.palette.Clear();
					foreach (var cont in compound.GetAsList("Palette").cont)
					{
						CompoundContainer block = (CompoundContainer)cont;
						var proto = BlockList.Find((string)block.Get("Name"));
						if (proto != null)
						{
							var bs = new BlockState(proto);
							if (block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
							section.palette.Add(bs);
						}
						else
						{
							section.palette.Add(BlockState.Unknown);
						}
					}

					if (section.palette.Count == 1 && !compound.Contains("BlockStates"))
					{
						//Do nothing, as all blocks already have an index of 0
					}
					else
					{
						//1.15 uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
						int indexLength = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
						long[] longs = (long[])compound.Get("BlockStates");
						string bits = "";
						for (int i = 0; i < longs.Length; i++)
						{
							string newBits = "";
							byte[] bytes = BitConverter.GetBytes(longs[i]);
							for (int j = 0; j < 8; j++)
							{
								newBits += Converter.ByteToBinary(bytes[j], true);
							}
							if (version == null || version.Value < Version.Release_1(16, 0))
							{
								bits += newBits;
							}
							else
							{
								bits += newBits.Substring(0, (int)Math.Floor(newBits.Length / (double)indexLength) * indexLength);
							}
						}
						//TODO: needs testing
						for (int y = 0; y < 16; y++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int x = 0; x < 16; x++)
								{
									section.blocks[x, y, z] = Converter.BitsToValue(bits, y * 256 + z * 16 + x, indexLength);
								}
							}
						}
					}
					chunk.sections.Add(secY, section);
				}
			}
			else
			{
				//TODO: needs refactoring, too much duplicated code
				var sectionsList = nbtCompound.GetAsList("sections");
				foreach (var o in sectionsList.cont)
				{
					var section = new ChunkSection(null);

					var compound = (CompoundContainer)o;
					if (!compound.Contains("Y") || !compound.Contains("block_states") || !compound.GetAsCompound("block_states").Contains("data")) continue;
					sbyte secY;
					unchecked
					{
						secY = (sbyte)compound.Get<byte>("Y");
						//secY = Convert.ToSByte(compound.Get("Y"));
					}
					var blockStates = compound.GetAsCompound("block_states");
					section.palette.Clear();
					foreach (var cont in blockStates.GetAsList("palette").cont)
					{
						CompoundContainer block = (CompoundContainer)cont;
						//HACK: bypass error throw
						var proto = BlockList.Find((string)block.Get("Name"), true);
						if (proto != null)
						{
							var bs = new BlockState(proto);
							if (block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
							section.palette.Add(bs);
						}
						else
						{
							section.palette.Add(BlockState.Unknown);
						}
					}

					if (section.palette.Count == 1 && !blockStates.Contains("data"))
					{
						//Do nothing, as all blocks already have an index of 0
					}
					else
					{
						//1.15 uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
						int indexLength = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
						long[] longs = blockStates.Get<long[]>("data");
						string bits = "";
						for (int i = 0; i < longs.Length; i++)
						{
							string newBits = "";
							byte[] bytes = BitConverter.GetBytes(longs[i]);
							for (int j = 0; j < 8; j++)
							{
								newBits += Converter.ByteToBinary(bytes[j], true);
							}
							if (version == null || version.Value < Version.Release_1(16, 0))
							{
								bits += newBits;
							}
							else
							{
								bits += newBits.Substring(0, (int)Math.Floor(newBits.Length / (double)indexLength) * indexLength);
							}
						}
						//TODO: needs testing
						for (int y = 0; y < 16; y++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int x = 0; x < 16; x++)
								{
									section.blocks[x, y, z] = Converter.BitsToValue(bits, y * 256 + z * 16 + x, indexLength);
								}
							}
						}
					}
					chunk.sections.Add(secY, section);
				}
			}
		}
		*/
	}
}

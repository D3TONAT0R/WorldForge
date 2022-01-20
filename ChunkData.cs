using MCUtils.Coordinates;
using MCUtils.IO;
using System;
using System.Collections.Generic;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class ChunkData
	{
		public ChunkCoord coords;

		public string defaultBlock = "minecraft:stone";
		public bool unlimitedHeight = false; //Allow blocks below 0 and above 256 (Versions 1.17+)
		public Region containingRegion;
		public Dictionary<sbyte, ChunkSection> sections = new Dictionary<sbyte, ChunkSection>();
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }
		public BiomeID[,] biomes = new BiomeID[16, 16];

		public List<Entity> entities = new List<Entity>();
		public Dictionary<BlockCoord, TileEntity> tileEntities = new Dictionary<BlockCoord, TileEntity>();

		public List<BlockCoord> blockTicks = new List<BlockCoord>();

		public NBTContent sourceNBT;

		private readonly object lockObj = new object();

		public ChunkData(Region region, ChunkCoord pos, string defaultBlock)
		{
			containingRegion = region;
			coords = pos;
			this.defaultBlock = defaultBlock;
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					biomes[x, y] = BiomeID.plains; //Defaults to plains biome
				}
			}
		}

		public ChunkData(Region region, NBTContent chunk)
		{
			containingRegion = region;
			sourceNBT = chunk;
			ChunkSerializer.ReadBlocksFromNBT(this, chunk.dataVersion);
			RecalculateSectionRange();
			ReadEntitiesAndTileEntitiesFromNBT(chunk.contents);
			ReadBiomesFromNBT(chunk);
		}

		///<summary>Sets the block at the given chunk coordinate</summary>
		public void SetBlockAt(int x, int y, int z, BlockState block)
		{
			if (!unlimitedHeight && (y < 0 || y > 255)) return;
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, block);
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)Math.Floor(y / 16f);
			if (!sections.ContainsKey(sectionY))
			{
				if (allowNew)
				{
					sections.Add(sectionY, new ChunkSection(defaultBlock));
					RecalculateSectionRange();
				}
				else
				{
					return null;
				}
			}
			return sections[sectionY];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given chunk coordinate. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlockAt(int x, int y, int z)
		{
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, 1); //1 is always the default block in a region generated from scratch
		}

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlockAt(int x, int y, int z)
		{
			var sec = GetChunkSectionForYCoord(y, false);
			if (sec == null) return BlockState.Air;
			return sec.GetBlockAt(x, y % 16, z);
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(int x, int y, int z, TileEntity te)
		{
			if (tileEntities == null)
			{
				tileEntities = new Dictionary<BlockCoord, TileEntity>();
			}
			tileEntities.Add(new BlockCoord(x, y, z), te);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(int x, int y, int z)
		{
			if (tileEntities == null) return null;
			var c = new BlockCoord(x, y, z);
			if (tileEntities.ContainsKey(c))
			{
				return tileEntities[c];
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(int x, int z)
		{
			return (BiomeID)biomes[x, z];
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int z, BiomeID biome)
		{
			lock (lockObj)
			{
				biomes[x, z] = biome;
			}
		}

		/// <summary>
		/// Marks the given chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(int x, int y, int z)
		{
			var coord = new BlockCoord(x, y, z);
			if (!blockTicks.Contains(coord))
			{
				blockTicks.Add(coord);
			}
		}

		/// <summary>
		/// Unmarks a previously marked chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(int x, int y, int z)
		{
			var coord = new BlockCoord(x, y, z);
			if (blockTicks.Contains(coord))
			{
				blockTicks.Remove(coord);
			}
		}

		///<summary>Reads all Entities and TileEntities from the given chunk NBT</summary>
		public void ReadEntitiesAndTileEntitiesFromNBT(CompoundContainer chunkLevelCompound)
		{
			if (chunkLevelCompound.Contains("Entities"))
			{
				var entList = chunkLevelCompound.GetAsList("Entities");
				if (entList != null && entList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < entList.Length; i++)
					{
						entities.Add(new Entity(entList.Get<CompoundContainer>(i)));
					}
				}
			}
			if (chunkLevelCompound.Contains("TileEntities"))
			{
				var tileEntList = chunkLevelCompound.GetAsList("TileEntities");
				if (tileEntList != null && tileEntList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < tileEntList.Length; i++)
					{
						var te = new TileEntity(tileEntList.Get<CompoundContainer>(i));
						tileEntities.Add(new BlockCoord(te.BlockPosX, te.BlockPosY, te.BlockPosZ), te);
					}
				}
			}
		}

		private void ReadBiomesFromNBT(NBTContent nbt)
		{
			if (nbt.contents.Contains("Biomes"))
			{
				var biomeArray = nbt.contents.Get<int[]>("Biomes");
				//Read topmost section, to avoid future cave biomes underground
				int offset = biomeArray.Length - 16;
				for (int x = 0; x < 4; x++)
				{
					for (int z = 0; z < 4; z++)
					{
						var value = (byte)biomeArray[offset + z * 4 + x];
						for (int x1 = 0; x1 < 4; x1++)
						{
							for (int z1 = 0; z1 < 4; z1++)
							{
								biomes[x * 4 + x1, z * 4 + z1] = (BiomeID)value;
							}
						}
					}
				}
			}
			else
			{
				//Default to plains biome
				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						biomes[x, z] = BiomeID.plains;
					}
				}
			}
		}


		///<summary>Converts the two-dimensional per-block biome array into a Minecraft compatible biome array (4x4x4 block volumes)</summary>
		private int[,,] MakeBiomeArray()
		{
			var finalBiomeArray = new int[4, 64, 4];
			for (int x = 0; x < 4; x++)
			{
				for (int z = 0; z < 4; z++)
				{
					var biome = GetPredominantBiomeIn4x4Area(x, z);
					for (int y = 0; y < 64; y++) finalBiomeArray[x, y, z] = (int)biome;
				}
			}
			return finalBiomeArray;
		}

		public short GetHighestBlock(int chunkX, int chunkZ, HeightmapType type)
		{
			short y = (short)(HighestSection * 16 + 15);
			while (y > LowestSection * 16)
			{
				if (Blocks.IsBlockForMap(GetBlockAt(chunkX, y, chunkZ).block, type)) return y;
				y--;
			}
			return short.MinValue;
		}

		public short GetHighestBlock(int chunkX, int chunkZ)
		{
			return GetHighestBlock(chunkX, chunkZ, HeightmapType.AllBlocks);
		}

		///<summary>Generates the full NBT data of a chunk</summary>
		public void WriteToNBT(CompoundContainer level, Version version)
		{
			ListContainer sectionsList = level.GetAsList("Sections");
			foreach (sbyte secY in sections.Keys)
			{
				var section = sections[secY];
				//if(IsSectionEmpty(secY)) continue;
				var comp = GetSectionCompound(sectionsList, secY);
				if (comp == null)
				{
					bool use_1_16_format = version >= Version.Release_1(16);
					comp = section.CreateCompound(secY, use_1_16_format);
				}
				sectionsList.Add(null, comp);
			}
			//Make the biomes
			var finalBiomeArray = MakeBiomeArray();
			List<int> biomes = new List<int>();
			for (int y = 0; y < 64; y++)
			{
				for (int z = 0; z < 4; z++)
				{
					for (int x = 0; x < 4; x++)
					{
						var b = finalBiomeArray != null ? finalBiomeArray[x, y, z] : 1;
						biomes.Add(b);
					}
				}
			}
			level.Add("Biomes", biomes.ToArray());

			//Add TileEntities
			var teList = level.GetAsList("TileEntities");
			foreach (var te in tileEntities)
			{
				teList.Add(te.Value.NBTCompound);
			}

			//Add Entities
			var entitiyList = level.GetAsList("Entities");
			foreach (var e in entities)
			{
				entitiyList.Add(e.NBTCompound);
			}

			//Add "post processing" positions (i.e. block positions that need an update)
			var ppList = level.GetAsList("PostProcessing");
			foreach(var t in blockTicks)
			{
				//TODO: section index and coordinate packing is being guessed, trying to find out how to do it.
				int listIndex = t.y / 16;
				var x = t.x % 16;
				var y = t.y % 16;
				var z = t.z % 16;
				var list = ppList.Get<ListContainer>(listIndex);
				short packed = (short)((z << 8) + (y << 4) + x);
				list.Add(packed);
			}

			//Add liquid / tile ticks
			/*
			var tileTickList = level.GetAsList("TileTicks");
			var liquidTickList = level.GetAsList("LiquidTicks");
			foreach (var t in blockTicks)
			{
				var block = GetBlockAt(t.x, t.y, t.z).block;
				bool isLiquid = block.IsLiquid;
				var comp = new CompoundContainer();
				string i;
				if(isLiquid)
				{
					//i = block.IsWater ? "minecraft:flowing_water" : "minecraft:flowing_lava";
					//flowing_water and flowing_lava only applies to non-source blocks.
					i = block.ID;
				}
				else
				{
					i = block.ID;
				}
				var worldCoord = coords.BlockCoord + t;
				comp.Add("i", i);
				comp.Add("t", 20);
				comp.Add("p", 0);
				comp.Add("x", worldCoord.x);
				comp.Add("y", worldCoord.y);
				comp.Add("z", worldCoord.z);
				if(isLiquid)
				{
					liquidTickList.Add(comp);
				}
				else
				{
					tileTickList.Add(comp);
				}
			}*/
		}

		///<summary>Writes the chunk's height data to a large heightmap at the given chunk coords</summary>
		public void WriteToHeightmap(short[,] hm, int x, int z, HeightmapType type)
		{
			if (!WriteHeightmapFromNBT(hm, x, z, type))
			{
				WriteHeightmapFromBlocks(hm, x, z, type);
			}
		}

		private void RecalculateSectionRange()
		{
			sbyte? lowest = null;
			sbyte? highest = null;
			for (sbyte s = -100; s < 127; s++)
			{
				if (lowest == null && sections.ContainsKey(s))
				{
					lowest = s;
				}
				if (sections.ContainsKey(s))
				{
					highest = s;
				}
			}
			LowestSection = lowest ?? 0;
			HighestSection = highest ?? 0;
		}

		private bool WriteHeightmapFromNBT(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			if (sourceNBT == null) return false;
			var chunkHM = sourceNBT.GetHeightmapFromChunkNBT(type);
			if (chunkHM == null) return false;
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = chunkHM[x, z];
				}
			}
			return true;
		}

		private void WriteHeightmapFromBlocks(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			sbyte highestSection = 127;
			while (highestSection > -127 && !sections.ContainsKey(highestSection))
			{
				highestSection--;
			}
			if (highestSection == -127) return;
			var sec = sections[highestSection];
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					short yTop;
					if (hm[x, z] != 0)
					{
						//The heightmap is already present, proceed from provided y value
						yTop = hm[x, z];
					}
					else
					{
						yTop = (short)(highestSection * 16 + 15);
					}
					for (short y = yTop; y > 0; y--)
					{
						var block = sec.GetBlockAt(x, y % 16, z);
						if (Blocks.IsBlockForMap(block.block, type))
						{
							if (block.block.Compare("minecraft:snow"))
							{
								//Go down to grass level in case of a snow layer
								y--;
							}
							hm[localChunkX * 16 + x, 511 - (localChunkZ * 16 + z)] = y;
							break;
						}
					}
				}
			}
		}

		private long BitsToLong(string bits)
		{
			bits = bits.PadLeft(64, '0');
			return Convert.ToInt64(bits, 2);
		}

		//TODO: How to deal with negative section Y values? (Minecraft 1.17+)
		private CompoundContainer GetSectionCompound(ListContainer sectionsList, sbyte y)
		{
			foreach (var o in sectionsList.cont)
			{
				var compound = (CompoundContainer)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				if ((byte)compound.Get("Y") == y) return compound;
			}
			return null;
		}

		private BiomeID GetPredominantBiomeIn4x4Area(int x, int z)
		{
			Dictionary<BiomeID, byte> occurences = new Dictionary<BiomeID, byte>();
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int z1 = 0; z1 < 4; z1++)
				{
					var b = biomes[x * 4 + x1, z * 4 + z1];
					if (!occurences.ContainsKey(b))
					{
						occurences.Add(b, 0);
					}
					occurences[b]++;
				}
			}
			BiomeID predominantBiome = 0;
			int predominantCells = 0;
			foreach (var k in occurences.Keys)
			{
				if (occurences[k] > predominantCells)
				{
					predominantCells = occurences[k];
					predominantBiome = k;
				}
			}
			return predominantBiome;
		}
	}
}
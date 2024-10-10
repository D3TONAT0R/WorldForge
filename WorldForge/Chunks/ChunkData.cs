using System;
using System.Collections.Generic;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using WorldForge.Entities;
using WorldForge.IO;
using WorldForge.NBT;
using WorldForge.Regions;
using WorldForge.TileEntities;

namespace WorldForge.Chunks
{
    public class ChunkData
	{
		public ChunkCoord WorldSpaceCoord
		{
			get
			{
				if(ParentRegion != null)
				{
					return new ChunkCoord(ParentRegion.regionPos.x * 32 + RegionSpaceCoord.x, ParentRegion.regionPos.z * 32 + RegionSpaceCoord.z);
				}
				else
				{
					return RegionSpaceCoord;
				}
			}
		}
		public Region ParentRegion { get; set; }
		public Dimension ParentDimension => ParentRegion?.ParentDimension;
		public World ParentWorld => ParentRegion?.ParentDimension?.ParentWorld;

		public ChunkCoord RegionSpaceCoord { get; set; }

		public ChunkStatus Status { get; set; } = ChunkStatus.light;


		public string defaultBlock = "minecraft:stone";
		public Dictionary<sbyte, ChunkSection> Sections { get; private set; }
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }
		public List<Entity> Entities { get; private set; }
		public Dictionary<BlockCoord, TileEntity> TileEntities { get; private set; }
		public List<BlockCoord> PostProcessTicks { get; private set; }
		public long InhabitedTime { get; set; } = 0;
		public GameVersion? ChunkGameVersion { get; private set; } = default;

		public NBTFile sourceNBT;

		public bool IsLoaded => Sections != null;
		public bool HasTerrain => Status >= ChunkStatus.surface;
		public bool HasFullyGeneratedTerrain => Status >= ChunkStatus.light;

		private readonly object lockObj = new object();

		public static ChunkData CreateNew(Region region, ChunkCoord regionSpacePos)
		{
			var c = new ChunkData(region, regionSpacePos);
			c.InitializeNewChunk();
			return c;
		}

		public static ChunkData CreateFromNBT(Region region, ChunkCoord regionSpacePos, NBTFile nbt, GameVersion? chunkGameVersion = null, bool loadContent = false)
		{
			var c = new ChunkData(region, regionSpacePos)
			{
				sourceNBT = nbt,
				ChunkGameVersion = chunkGameVersion
			};
			if(loadContent) c.Load();
			return c;
		}

		private ChunkData(Region containingRegion, ChunkCoord localPos)
		{
			ParentRegion = containingRegion;
			RegionSpaceCoord = localPos;
		}

		public void InitializeNewChunk()
		{
			Sections = new Dictionary<sbyte, ChunkSection>();
			TileEntities = new Dictionary<BlockCoord, TileEntity>();
			Entities = new List<Entity>();
			PostProcessTicks = new List<BlockCoord>();
		}

		/// <summary>
		/// Loads the chunk from the region file
		/// </summary>
		public void Load()
		{
			if(IsLoaded) throw new InvalidOperationException("Chunk is already loaded");

			if(sourceNBT != null && sourceNBT.dataVersion.HasValue)
			{
				ChunkGameVersion = GameVersion.FromDataVersion(sourceNBT.dataVersion.Value).Value;
			}
			else
			{
				if(ParentWorld != null)
				{
					ChunkGameVersion = ParentWorld.GameVersion;
				}
			}

			InitializeNewChunk();

			var chunkSerializer = ChunkSerializer.GetForVersion(ChunkGameVersion ?? GameVersion.FirstVersion);
			chunkSerializer.ReadChunkNBT(this, ChunkGameVersion);
		}

		/// <summary>
		/// Sets the block at the given chunk coordinate
		/// </summary>
		public void SetBlockAt(BlockCoord pos, BlockState block)
		{
			if(!IsLoaded) Load();
			GetChunkSectionForYCoord(pos.y, true).SetBlockAt(pos.x, pos.y.Mod(16), pos.z, block);
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)Math.Floor(y / 16f);
			if (!Sections.ContainsKey(sectionY))
			{
				if (allowNew)
				{
					Sections.Add(sectionY, new ChunkSection(this, defaultBlock));
					RecalculateSectionRange();
				}
				else
				{
					return null;
				}
			}
			return Sections[sectionY];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given chunk coordinate. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlockAt(BlockCoord pos)
		{
			if(!IsLoaded) Load();
			GetChunkSectionForYCoord(pos.y, true).SetBlockAt(pos.x, pos.y.Mod(16), pos.z, 1); //1 is always the default block in a region generated from scratch
		}

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlockAt(BlockCoord pos)
		{
			if (!HasTerrain) return null;
			if(!IsLoaded) Load();
			var sec = GetChunkSectionForYCoord(pos.y, false);
			if (sec == null) return null;
			return sec.GetBlockAt(pos.x, pos.y.Mod(16), pos.z);
		}

		public int ForEachBlock(short yMin, short yMax, Action<BlockCoord, BlockState> action)
		{
			if (!HasTerrain) return 0;
			if(!IsLoaded) Load();
			int countedBlocks = 0;
			foreach (var kv in Sections)
			{
				var section = kv.Value;
				short baseY = (short)(kv.Key * 16);
				if (baseY > yMax || baseY + 15 < yMin) continue;

				for (byte y = 0; y < 16; y++)
				{
					short worldY = (short)(baseY + y);
					if (worldY < yMin || worldY >= yMax) continue;
					for (byte z = 0; z < 16; z++)
					{
						for (byte x = 0; x < 16; x++)
						{
							var b = section.GetBlockAt(x, y, z);
							action.Invoke(new BlockCoord(x, baseY + y, z), b);
							countedBlocks++;
						}
					}
				}
			}
			return countedBlocks;
		}

		public int ForEachBlock(Action<BlockCoord, BlockState> action)
		{
			return ForEachBlock((short)(LowestSection * 16), (short)(HighestSection * 16 + 15), action);
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(BlockCoord pos, TileEntity te)
		{
			if(!IsLoaded) Load();
			if (TileEntities == null)
			{
				TileEntities = new Dictionary<BlockCoord, TileEntity>();
			}
			TileEntities.Add(pos, te);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			if (!IsLoaded) Load();
			if (TileEntities == null) return null;
			if (TileEntities.ContainsKey(pos))
			{
				return TileEntities[pos];
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(BlockCoord pos, BiomeID biome)
		{
			if(!IsLoaded) Load();
			var section = GetChunkSectionForYCoord(pos.y, false);
			if (section != null)
			{
				section.SetBiomeAt(pos.x, pos.y.Mod(16), pos.z, biome);
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int z, BiomeID biome)
		{
			if(!IsLoaded) Load();
			foreach (var sec in Sections.Values)
			{
				sec.SetBiomeColumnAt(x, z, biome);
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(BlockCoord pos)
		{
			if(!IsLoaded) Load();
			var section = GetChunkSectionForYCoord(pos.y, false);
			if (section != null)
			{
				return section.GetBiomeAt(pos.x, pos.y.Mod(16), pos.z);
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(int x, int z)
		{
			if(!IsLoaded) Load();
			sbyte highestSectionWithBiomeData = HighestSection;
			while (highestSectionWithBiomeData > 0 && (!Sections.TryGetValue(highestSectionWithBiomeData, out var s) || !s.HasBiomesDefined))
			{
				highestSectionWithBiomeData--;
			}
			return GetBiomeAt((x, highestSectionWithBiomeData * 16 + 15, z));
		}

		/// <summary>
		/// Marks the given chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			if(!IsLoaded) Load();
			if (!PostProcessTicks.Contains(pos))
			{
				PostProcessTicks.Add(pos);
			}
		}

		/// <summary>
		/// Unmarks a previously marked chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			if(!IsLoaded) Load();
			if (PostProcessTicks.Contains(pos))
			{
				PostProcessTicks.Remove(pos);
			}
		}

		public short GetHighestBlock(int x, int z, HeightmapType type = HeightmapType.AllBlocks)
		{
			if(!IsLoaded) Load();
			short y = (short)(HighestSection * 16 + 15);
			while (y >= LowestSection * 16)
			{
				var blockState = GetBlockAt((x.Mod(16), y, z.Mod(16)));
				if (blockState != null && Blocks.IsBlockForMap(blockState.block, type)) return y;
				y--;
			}
			return short.MinValue;
		}

		public short[,] GetHeightmap(HeightmapType type, bool forceManualCalculation = false)
		{
			short[,] hm = null;
			if(!forceManualCalculation)
			{
				hm = sourceNBT?.GetHeightmapFromChunkNBT(type);
			}
			if(hm == null)
			{
				//Calculate heightmap manually
				hm = new short[16, 16];
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x++)
					{
						hm[x, z] = GetHighestBlock(x, z, type);
					}
				}
			}
			return hm;
		}

		///<summary>Writes the chunk's height data to a large heightmap at the given chunk coords</summary>
		public void WriteToHeightmap(short[,] hm, int x, int z, HeightmapType type)
		{
			if(!IsLoaded) Load();
			if (!WriteHeightmapFromNBT(hm, x, z, type))
			{
				WriteHeightmapFromBlocks(hm, x, z, type);
			}
		}

		public void RecalculateSectionRange()
		{
			sbyte? lowest = null;
			sbyte? highest = null;
			for (sbyte s = -100; s < 127; s++)
			{
				if (lowest == null && Sections.ContainsKey(s))
				{
					lowest = s;
				}
				if (Sections.ContainsKey(s))
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
			/*sbyte highestSection = 127;
			while (highestSection > -127 && !sections.ContainsKey(highestSection))
			{
				highestSection--;
			}
			if (highestSection == -127) return;
			var sec = sections[highestSection];
			*/
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = GetHighestBlock(x, z, type);
					/*
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
						var block = sec.GetBlockAt(x, y.Mod(16), z);
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
					*/
				}
			}
		}

		//TODO: How to deal with negative section Y values? (Minecraft 1.17+)
		private NBTCompound GetSectionCompound(NBTList sectionsList, sbyte y)
		{
			foreach (var o in sectionsList.listContent)
			{
				var compound = (NBTCompound)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				if ((byte)compound.Get("Y") == y) return compound;
			}
			return null;
		}
	}
}
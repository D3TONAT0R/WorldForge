using System;
using System.Collections.Concurrent;
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
	public class Chunk
	{
		public ChunkCoord WorldSpaceCoord
		{
			get
			{
				if (ParentRegion != null)
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
		public Dimension ParentDimension => ParentRegion?.Parent;
		public World ParentWorld => ParentRegion?.Parent?.ParentWorld;

		public ChunkCoord RegionSpaceCoord { get; set; }

		public ChunkStatus Status { get; set; } = ChunkStatus.light;

		public ConcurrentDictionary<sbyte, ChunkSection> Sections { get; private set; }
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }
		public List<Entity> Entities { get; private set; }
		public Dictionary<BlockCoord, TileEntity> TileEntities { get; private set; }
		public List<BlockCoord> PostProcessTicks { get; private set; }
		public List<POI> POIs { get; private set; }
		public long InhabitedTime { get; set; } = 0;
		public GameVersion? ChunkGameVersion { get; private set; } = default;

		/// <summary>
		/// Contains the raw NBT data of the chunk, if it was loaded from a file and not yet fully loaded.
		/// </summary>
		public ChunkSourceData SourceData { get; private set; }

		public bool IsLoaded => Sections != null;
		public bool HasTerrain => Status >= ChunkStatus.surface;
		public bool HasFullyGeneratedTerrain => Status >= ChunkStatus.light;
		public ChunkLoadFlags LoadFlags { get; set; }

		#region Creation methods

		public static Chunk CreateNew(Region region, ChunkCoord regionSpacePos)
		{
			var c = new Chunk(region, regionSpacePos);
			c.InitializeNewChunk();
			return c;
		}

		public static Chunk CreateFromNBT(Region region, ChunkCoord regionSpacePos, ChunkSourceData data,
			GameVersion? versionHint = null, bool loadContent = false, ChunkLoadFlags loadFlags = ChunkLoadFlags.All)
		{
			var c = new Chunk(region, regionSpacePos)
			{
				SourceData = data,
				ChunkGameVersion = versionHint
			};
			if (loadContent) c.Load(loadFlags);
			return c;
		}

		private Chunk(Region containingRegion, ChunkCoord localPos)
		{
			ParentRegion = containingRegion;
			RegionSpaceCoord = localPos;
		}

		public Chunk Clone()
		{
			var clone = new Chunk(ParentRegion, RegionSpaceCoord);
			clone.Status = Status;
			if (Sections != null)
			{
				clone.Sections = new ConcurrentDictionary<sbyte, ChunkSection>();
				foreach (var sec in Sections)
				{
					clone.Sections[sec.Key] = sec.Value.Clone();
				}
			}
			clone.LowestSection = LowestSection;
			clone.HighestSection = HighestSection;
			if (Entities != null) clone.Entities = new List<Entity>(Entities); //TODO: clone entities
			if (TileEntities != null) clone.TileEntities = new Dictionary<BlockCoord, TileEntity>(TileEntities); //TODO: clone tile entities
			if (PostProcessTicks != null) clone.PostProcessTicks = new List<BlockCoord>(PostProcessTicks);
			if (POIs != null) clone.POIs = new List<POI>(POIs); //TODO: clone POIs
			clone.InhabitedTime = InhabitedTime;
			clone.ChunkGameVersion = ChunkGameVersion;
			clone.SourceData = SourceData;
			clone.LoadFlags = LoadFlags;
			return clone;
		}

		public void InitializeNewChunk()
		{
			Sections = new ConcurrentDictionary<sbyte, ChunkSection>();
			TileEntities = new Dictionary<BlockCoord, TileEntity>();
			Entities = new List<Entity>();
			PostProcessTicks = new List<BlockCoord>();
			POIs = new List<POI>();
		}

		/// <summary>
		/// Loads the chunk from the region file
		/// </summary>
		public void Load(ChunkLoadFlags loadFlags = ChunkLoadFlags.All, ExceptionHandling exceptionHandling = ExceptionHandling.Throw)
		{
			if (IsLoaded) throw new InvalidOperationException("Chunk is already loaded");
			if (SourceData == null) throw new InvalidOperationException("No source data available to load chunk");

			if (SourceData.main.dataVersion.HasValue)
			{
				ChunkGameVersion = GameVersion.FromDataVersion(SourceData.main.dataVersion.Value).Value;
			}
			else
			{
				if (ParentWorld != null)
				{
					ChunkGameVersion = ParentWorld.GameVersion;
				}
			}

			if (SourceData.sourceRegionType == ChunkSourceData.SourceRegionType.MCRegion && ChunkGameVersion >= GameVersion.FirstAnvilVersion)
			{
				// Impossible chunk version - revert to last MCR version
				ChunkGameVersion = GameVersion.LastMCRVersion;
			}
			if(SourceData.sourceRegionType == ChunkSourceData.SourceRegionType.AlphaChunk && ChunkGameVersion > GameVersion.LastAlphaChunksVersion)
			{
				// Impossible chunk version - revert to last alpha chunk version
				ChunkGameVersion = GameVersion.LastAlphaChunksVersion;
			}

			InitializeNewChunk();

			ChunkSerializer chunkSerializer;
			GameVersion? versionHint = ChunkGameVersion ?? ParentRegion?.versionHint ?? ParentWorld?.GameVersion;
			if (versionHint.HasValue)
			{
				chunkSerializer = ChunkSerializer.GetForVersion(versionHint.Value);
			}
			else
			{
				if (SourceData.main.dataVersion.HasValue)
				{
					chunkSerializer = ChunkSerializer.GetForDataVersion(SourceData.main.dataVersion.Value);
				}
				else
				{
					//TODO: how to load alpha chunks?
					var mcr = ParentRegion?.sourceFilePaths.mainPath.ToLower().EndsWith(".mcr") ?? false;
					chunkSerializer = mcr ?
						//Use the beta serializer for MCR files
						ChunkSerializer.GetOrCreateSerializer<ChunkSerializerBeta>(GameVersion.FirstMCRVersion) :
						//Assume anvil format
						ChunkSerializer.GetForChunkNBT(SourceData.main.contents);
				}
			}
			Logger.Verbose($"Loading chunk {WorldSpaceCoord} using serializer {chunkSerializer.GetType().Name} (chunk game version: {ChunkGameVersion})");
			chunkSerializer.ReadChunkNBT(this, SourceData, ChunkGameVersion, loadFlags, exceptionHandling);
			SourceData = null;
		}

		#endregion

		#region Block related methods

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlock(BlockCoord pos)
		{
			if (!HasTerrain) return null;
			if (!IsLoaded) Load();
			var sec = GetChunkSectionForYCoord(pos.y, false);
			if (sec == null) return null;
			return sec.GetBlock(pos.x, pos.y & 0xF, pos.z);
		}

		/// <summary>
		/// Sets the block at the given chunk coordinate
		/// </summary>
		public void SetBlock(BlockCoord pos, BlockState block)
		{
			if (!IsLoaded) Load();
			GetChunkSectionForYCoord(pos.y, true).SetBlock(pos.x, pos.y & 0xF, pos.z, block);
		}

		public void SetBlock(BlockCoord pos, BlockID block)
		{
			if (!IsLoaded) Load();
			var state = BlockState.Simple(block);
			GetChunkSectionForYCoord(pos.y, true).SetBlock(pos.x, pos.y & 0xF, pos.z, state);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			if (!IsLoaded) Load();
			if (TileEntities == null) return null;
			pos += WorldSpaceCoord.BlockCoord;
			if (TileEntities.ContainsKey(pos))
			{
				return TileEntities[pos];
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(BlockCoord pos, TileEntity te)
		{
			if (!IsLoaded) Load();
			if (TileEntities == null)
			{
				TileEntities = new Dictionary<BlockCoord, TileEntity>();
			}
			TileEntities.Add(pos, te);
		}

		public bool SetBlockWithTileEntity(BlockCoord pos, BlockState block, TileEntity te)
		{
			SetBlock(pos, block);
			SetTileEntity(pos, te);
			return true;
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)MathUtils.FastDivFloor(y, 16);
			if (Sections.TryGetValue(sectionY, out var section))
			{
				return section;
			}
			if (allowNew)
			{
				if (Sections.TryAdd(sectionY, new ChunkSection(this)))
				{
					RecalculateSectionRange();
				}
				return Sections[sectionY];
			}
			return null;
		}

		#endregion

		#region Biome related methods

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(BlockCoord pos)
		{
			if (!IsLoaded) Load();
			var section = GetChunkSectionForYCoord(pos.y, false);
			if (section != null)
			{
				return section.GetBiome(pos.x, pos.y & 0xF, pos.z);
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(int x, int z)
		{
			if (!IsLoaded) Load();
			sbyte highestSectionWithBiomeData = HighestSection;
			while (highestSectionWithBiomeData > 0 && (!Sections.TryGetValue(highestSectionWithBiomeData, out var s) || !s.HasBiomesDefined))
			{
				highestSectionWithBiomeData--;
			}
			return GetBiomeAt((x, highestSectionWithBiomeData * 16 + 15, z));
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(BlockCoord pos, BiomeID biome)
		{
			if (!IsLoaded) Load();
			var section = GetChunkSectionForYCoord(pos.y, false);
			if (section != null)
			{
				section.SetBiome(pos.x, pos.y & 0xF, pos.z, biome);
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int z, BiomeID biome)
		{
			if (!IsLoaded) Load();
			foreach (var sec in Sections)
			{
				sec.Value.SetBiomeColumn(x, z, biome);
			}
		}

		#endregion

		#region Tick update related methods

		/// <summary>
		/// Marks the given chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			if (!IsLoaded) Load();
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
			if (!IsLoaded) Load();
			if (PostProcessTicks.Contains(pos))
			{
				PostProcessTicks.Remove(pos);
			}
		}

		#endregion

		#region POI related methods

		public void SetPOI(BlockCoord pos, POI poi)
		{
			if (!IsLoaded) Load();
			int i = GetPOIIndex(pos);
			if (i >= 0) POIs[i] = poi;
			else POIs.Add(poi);
		}

		public POI GetPOIAt(BlockCoord pos)
		{
			if (!IsLoaded) Load();
			int i = GetPOIIndex(pos);
			return i >= 0 ? POIs[i] : null;
		}

		private int GetPOIIndex(BlockCoord pos)
		{
			for (int i = 0; i < POIs.Count; i++)
			{
				if (POIs[i].position == pos) return i;
			}
			return -1;
		}

		#endregion

		#region Convenience methods

		public short GetHighestBlock(int x, int z, HeightmapType type = HeightmapType.AllBlocks)
		{
			if (!IsLoaded) Load();
			short y = (short)(HighestSection * 16 + 15);
			while (y >= LowestSection * 16)
			{
				var blockState = GetBlock((x & 0xF, y, z & 0xF));
				if (blockState != null && Blocks.IsBlockForMap(blockState.Block, type)) return y;
				y--;
			}
			return short.MinValue;
		}

		public bool FindBlock(BlockState block, out BlockCoord pos)
		{
			foreach (var s in Sections)
			{
				if (s.Value == null) continue;
				var index = s.Value.GetPaletteIndex(block);
				if (index >= 0)
				{
					for (int i = 0; i < s.Value.blocks.Length; i++)
					{
						if (s.Value.blocks[i] == index)
						{
							pos = ChunkSection.GetPositionFromArrayIndex(i);
							pos.y += s.Key * 16;
							return true;
						}
					}
				}
			}
			pos = BlockCoord.Zero;
			return false;
		}

		public short[,] GetHeightmap(HeightmapType type, bool forceManualCalculation = false)
		{
			short[,] hm = null;
			if (!forceManualCalculation && SourceData != null)
			{
				hm = NBTSerializer.GetHeightmapFromChunkNBT(SourceData.main, type, ChunkGameVersion ?? GameVersion.FirstAnvilVersion, ParentDimension);
			}
			if (hm == null)
			{
				//Calculate heightmap manually
				hm = new short[16, 16];
				for (int z = 0; z < 16; z++)
				{
					for (int x = 0; x < 16; x++)
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
			if (!IsLoaded) Load();
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

		public int ForEachBlock(short yMin, short yMax, Action<BlockCoord, BlockState> action)
		{
			if (!HasTerrain) return 0;
			if (!IsLoaded) Load();
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
							var b = section.GetBlock(x, y, z);
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

		#endregion

		private bool WriteHeightmapFromNBT(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			if (SourceData == null) return false;
			var chunkHM = NBTSerializer.GetHeightmapFromChunkNBT(SourceData.main, type, ChunkGameVersion ?? GameVersion.FirstAnvilVersion, ParentDimension);
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
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = GetHighestBlock(x, z, type);
				}
			}
		}
	}
}
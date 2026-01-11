using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.TileEntities;

namespace WorldForge.Regions
{
	public class Region
	{
		public readonly RegionLocation regionPos;
		public readonly RegionFilePaths sourceFilePaths;

		public GameVersion? versionHint;
		public Chunk[,] chunks;
		public List<Chunk> orphanChunks;

		public Dimension Parent { get; internal set; }
		public World ParentWorld => Parent?.ParentWorld;

		public bool IsLoaded => chunks != null;

		public RegionFileDataPositions RegionFileInfo { get; internal set; }

		public int ChunkCount
		{
			get
			{
				if(chunks == null) return 0;
				int count = 0;
				for(int x = 0; x < 32; x++)
				{
					for(int z = 0; z < 32; z++)
					{
						if(chunks[x, z] != null) count++;
					}
				}
				return count;
			}
		}
		
		public int LoadedChunkCount
		{
			get
			{
				if(chunks == null) return 0;
				int count = 0;
				for(int x = 0; x < 32; x++)
				{
					for(int z = 0; z < 32; z++)
					{
						if(chunks[x, z] != null && chunks[x, z].IsLoaded) count++;
					}
				}
				return count;
			}
		}

		#region Creation methods

		public static Region CreateNew(RegionLocation loc, Dimension parent)
		{
			Region r = new Region(loc, parent, null);
			r.InitializeChunks();
			return r;
		}

		public static Region CreateExisting(RegionLocation loc, Dimension parent, RegionFilePaths paths)
		{
			Region r = new Region(loc, parent, paths);
			return r;
		}

		public static Region CreateExisting(RegionLocation loc, Dimension parent, string path)
		{
			Region r = new Region(loc, parent, new RegionFilePaths(path, null, null));
			return r;
		}

		private Region(RegionLocation loc, Dimension parent, RegionFilePaths sourceFilePaths)
		{
			regionPos = loc;
			Parent = parent;
			this.sourceFilePaths = sourceFilePaths;
		}

		public void InitializeChunks()
		{
			chunks = new Chunk[32, 32];
		}

		public void Load(bool loadChunks = false, bool loadOrphanChunks = false, ChunkLoadFlags loadFlags = ChunkLoadFlags.All)
		{
			if(IsLoaded) throw new InvalidOperationException("Region is already loaded.");
			RegionDeserializer.LoadRegionContent(this, loadChunks, loadOrphanChunks, loadFlags);
		}

		private void LoadIfRequired(ChunkLoadFlags loadFlags = ChunkLoadFlags.All)
		{
			if(!IsLoaded)
			{
				Load(loadFlags: loadFlags);
			}
		}

		/// <summary>
		/// Loads all chunks that were previously unloaded.
		/// </summary>
		public void LoadAllChunks(ChunkLoadFlags chunkLoadFlags = ChunkLoadFlags.All)
		{
			LoadIfRequired();
			Parallel.For(0, 32, WorldForgeManager.ParallelOptions, x =>
			{
				for(int z = 0; z < 32; z++)
				{
					var c = chunks[x, z];
					if(c != null && !c.IsLoaded)
					{
						c.Load(chunkLoadFlags);
					}
				}
			});
		}

		#endregion

		#region Chunk related methods


		/// <summary>
		/// Gets the chunk containing the block's position
		/// </summary>
		public Chunk GetChunkAtBlock(BlockCoord coord, bool allowNewChunks)
		{
			LoadIfRequired();
			var chunk = coord.Chunk.LocalRegionPos;
			if(allowNewChunks && chunks[chunk.x, chunk.z] == null)
			{
				chunks[chunk.x, chunk.z] = Chunk.CreateNew(this, new ChunkCoord(chunk.x, chunk.z));
			}
			return chunks[chunk.x, chunk.z];
		}

		public bool TryGetChunkAtBlock(BlockCoord coord, out Chunk chunk)
		{
			LoadIfRequired();
			var c = coord.Chunk.LocalRegionPos;
			chunk = chunks[c.x, c.z];
			return chunk != null;
		}

		public Chunk GetChunk(int x, int z, ChunkLoadFlags flags = ChunkLoadFlags.All)
		{
			LoadIfRequired(flags);
			return chunks[x, z];
		}

		public bool TryGetChunk(int x, int z, out Chunk chunk)
		{
			LoadIfRequired();
			chunk = chunks[x, z];
			return chunk != null;
		}

		#endregion

		#region Block related methods

		///<summary>Returns true if the given locations contains air or the section has not been generated yet</summary>
		public bool IsAir(BlockCoord pos)
		{
			var b = GetBlock(pos);
			return b == null || b.IsAir;
		}

		///<summary>Gets the block type at the given location.</summary>
		public BlockID GetBlock(BlockCoord pos)
		{
			var chunk = GetChunkAtBlock(pos, false);
			if (chunk != null)
			{
				return chunk.GetBlock(pos.LocalChunkCoords)?.Block ?? Blocks.air;
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(BlockCoord pos)
		{
			return GetChunkAtBlock(pos, false)?.GetBlock(pos.LocalChunkCoords);
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockID block, bool allowNewChunks = false)
		{
			var chunk = GetChunkAtBlock(pos, allowNewChunks);
			if(chunk != null)
			{
				chunk.SetBlock(pos.LocalChunkCoords, block);
				return true;
			}
			return false;
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockState block, bool allowNewChunks = false)
		{
			var chunk = GetChunkAtBlock(pos, allowNewChunks);
			if(chunk != null)
			{
				chunk.SetBlock(pos.LocalChunkCoords, block);
				return true;
			}
			return false;
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			return GetChunkAtBlock(pos, true)?.GetTileEntity(pos.LocalChunkCoords);
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(BlockCoord pos, TileEntity te)
		{
			var chunk = GetChunkAtBlock(pos, true);
			chunk?.SetTileEntity(pos.LocalChunkCoords, te);
			return chunk != null;
		}

		public bool SetBlockWithTileEntity(BlockCoord pos, BlockState block, TileEntity te, bool allowNewChunks)
		{
			var chunk = GetChunkAtBlock(pos, allowNewChunks);
			if(chunk != null)
			{
				return chunk.SetBlockWithTileEntity(pos.LocalChunkCoords, block, te);
			}
			return false;
		}

		#endregion

		#region Biome related methods
		///<summary>Gets the biome at the given location.</summary>
		public BiomeID GetBiomeAt(int x, int z)
		{
			var chunk = GetChunkAtBlock(new BlockCoord(x, 0, z), false);
			if (chunk != null)
			{
				//for (int y = 0; y < 256; y++)
				//{
				return chunk.GetBiomeAt(x & 0xF, z & 0xF);
				//}
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID GetBiomeAt(BlockCoord pos)
		{
			var chunk = GetChunkAtBlock(pos, false);
			if (chunk != null)
			{
				return chunk.GetBiomeAt(pos.LocalChunkCoords);
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiomeAt(int x, int z, BiomeID biome)
		{
			var chunk = GetChunkAtBlock(new BlockCoord(x, 0, z), false);
			if (chunk != null)
			{
				chunk.SetBiomeAt(x & 0xF, z & 0xF, biome);
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiomeAt(BlockCoord pos, BiomeID biome)
		{
			var chunk = GetChunkAtBlock(pos, false);
			if (chunk != null)
			{
				chunk.SetBiomeAt(pos.LocalChunkCoords, biome);
			}
		}

		#endregion

		#region Tick update related methods

		/// <summary>
		/// Marks the given coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			var chunk = GetChunkAtBlock(pos, false);
			if (chunk != null)
			{
				chunk.MarkForTickUpdate(pos.LocalChunkCoords);
			}
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when this respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			var chunk = GetChunkAtBlock(pos, false);
			if (chunk != null)
			{
				chunk.UnmarkForTickUpdate(pos.LocalChunkCoords);
			}
		}

		#endregion

		#region Convenience methods

		/// <summary>
		/// Gets the highest block at the given location.
		/// </summary>
		public short GetHighestBlock(int x, int z, HeightmapType heightmapType = HeightmapType.AllBlocks)
		{
			var chunk = GetChunkAtBlock(new BlockCoord(x, 0, z), false);
			if (chunk != null)
			{
				return chunk.GetHighestBlock(x & 0xF, z & 0xF, heightmapType);
			}
			else
			{
				return short.MinValue;
			}
		}

		///<summary>Generates a heightmap by reading the chunk's heightmaps or calculating it from existing blocks.</summary>
		public short[,] GetHeightmapFromNBT(HeightmapType type)
		{
			LoadIfRequired();
			short[,] hm = new short[512, 512];
			for (int x = 0; x < 32; x++)
			{
				for (int z = 0; z < 32; z++)
				{
					var c = chunks[x, z];
					if (c != null)
					{
						c.WriteToHeightmap(hm, x, z, type);
					}
				}
			}
			return hm;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks.
		/// </summary>
		public int GetWaterDepth(BlockCoord pos)
		{
			int depth = 0;
			var block = GetBlock(pos);
			while (block.IsWater)
			{
				depth++;
				pos.y--;
				block = GetBlock(pos);
			}
			return depth;
		}

		public IEnumerable<(BlockCoord, TileEntity)> EnumerateTileEntities()
		{
			LoadIfRequired();
			foreach (var c in chunks)
			{
				if (c != null)
				{
					foreach (var pos in c.TileEntities.Keys)
					{
						yield return (pos, c.TileEntities[pos]);
					}
				}
			}
		}

		public bool ContainsPosition(BlockCoord2D pos)
		{
			int localX = pos.x - regionPos.x * 512;
			int localZ = pos.z - regionPos.z * 512;
			return pos.x >= 0 && pos.x < 512 && pos.z >= 0 && pos.z < 512;
		}

		#endregion

		public void SaveToFiles(string destinationDirectory, GameVersion gameVersion, string name = null, FileMode fileMode = FileMode.Create)
		{
			LoadIfRequired();
			if (name == null) name = regionPos.ToFileName();
			bool separateFiles = gameVersion >= GameVersion.Release_1(13);
			var mainPath = Path.Combine(destinationDirectory, "region", name);
			var entitiesPath = separateFiles ? Path.Combine(destinationDirectory, "entities", name) : null;
			var poiPath = separateFiles ? Path.Combine(destinationDirectory, "poi", name) : null;
			RegionSerializer.WriteRegionFiles(this, gameVersion, mainPath, entitiesPath, poiPath);
		}

		public void SaveMainRegionFile(string rootDirectory, GameVersion gameVersion, string name = null, FileMode fileMode = FileMode.Create)
		{
			LoadIfRequired();
			var path = Path.Combine(rootDirectory, name);
			RegionSerializer.WriteRegionFiles(this, gameVersion, path, null, null);
		}
	}
}
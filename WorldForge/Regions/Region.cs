using System;
using System.Collections.Generic;
using System.IO;
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
		public readonly string sourceFilePath;

		public GameVersion? versionHint;
		public Chunk[,] chunks;
		public List<Chunk> orphanChunks;

		public Dimension Parent { get; internal set; }
		public World ParentWorld => Parent?.ParentWorld;

		public bool IsLoaded => chunks != null;

		public RegionFileDataPositions RegionFileInfo { get; internal set; }

		private Region(RegionLocation loc, Dimension parent, string sourcePath)
		{
			regionPos = loc;
			Parent = parent;
			sourceFilePath = sourcePath;
		}

		public static Region CreateNew(RegionLocation loc, Dimension parent)
		{
			Region r = new Region(loc, parent, null);
			r.InitializeChunks();
			return r;
		}

		public static Region CreateExisting(RegionLocation loc, Dimension parent, string sourceFilePath)
		{
			Region r = new Region(loc, parent, sourceFilePath);
			return r;
		}

		public void InitializeChunks()
		{
			chunks = new Chunk[32, 32];
		}

		public void Load(bool loadChunks = false, bool loadOrphanChunks = false)
		{
			if(IsLoaded) throw new InvalidOperationException("Region is already loaded.");
			RegionDeserializer.LoadRegionContent(this, loadChunks, loadOrphanChunks);
		}

		private void LoadIfRequired()
		{
			if(!IsLoaded) Load();
		}

		/// <summary>
		/// Loads all chunks that were previously unloaded.
		/// </summary>
		public void LoadAllChunks()
		{
			LoadIfRequired();
			for(int x = 0; x < 32; x++)
			{
				for(int z = 0; z < 32; z++)
				{
					var c = chunks[x, z];
					if(c != null && !c.IsLoaded)
					{
						c.Load();
					}
				}
			}
		}

		public bool ContainsPosition(int x, int z)
		{
			int localX = x - regionPos.x * 512;
			int localZ = z - regionPos.z * 512;
			return x >= 0 && x < 512 && z >= 0 && z < 512;
		}

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
				return chunk.GetBlockAt(pos.LocalChunkCoords)?.Block ?? BlockList.Find("minecraft:air");
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(BlockCoord pos)
		{
			return GetChunkAtBlock(pos, false)?.GetBlockAt(pos.LocalChunkCoords);
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			return GetChunkAtBlock(pos, true)?.GetTileEntity(pos.LocalChunkCoords);
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockState block, bool allowNewChunks = false)
		{
			GetChunkAtBlock(pos, allowNewChunks)?.SetBlockAt(pos.LocalChunkCoords, block);
			return true;
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockID block, bool allowNewChunks = false)
		{
			return SetBlock(pos, new BlockState(block), allowNewChunks);
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, string block, bool allowNewChunks = false)
		{
			return SetBlock(pos, BlockList.Find(block), allowNewChunks);
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(BlockCoord pos, TileEntity te)
		{
			var chunk = GetChunkAtBlock(pos, true);
			chunk?.SetTileEntity(pos.LocalChunkCoords, te);
			return chunk != null;
		}

		/// <summary>
		/// Gets the chunk containing the block's position
		/// </summary>
		public Chunk GetChunkAtBlock(BlockCoord coord, bool allowNewChunks)
		{
			LoadIfRequired();
			var chunk = coord.Chunk.LocalRegionPos;
			if (allowNewChunks && chunks[chunk.x, chunk.z] == null)
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

		public Chunk GetChunk(int x, int z)
		{
			LoadIfRequired();
			return chunks[x, z];
		}

		public bool TryGetChunk(int x, int z, out Chunk chunk)
		{
			LoadIfRequired();
			chunk = chunks[x, z];
			return chunk != null;
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(BlockCoord pos, bool allowNewChunks = false)
		{
			LoadIfRequired();
			int chunkX = (int)Math.Floor(pos.x / 16.0);
			int chunkZ = (int)Math.Floor(pos.z / 16.0);
			if (chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return;
			if (chunks[chunkX, chunkZ] == null && allowNewChunks)
			{
				chunks[chunkX, chunkZ] = Chunk.CreateNew(this, new ChunkCoord(chunkX, chunkZ));
			}
			var c = chunks[chunkX, chunkZ];
			if (c != null) c.SetDefaultBlockAt(pos.LocalChunkCoords);
		}

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
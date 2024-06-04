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

		public RegionPOI poiStorage;
		public RegionEntities entitiyStorage;

		public byte[,] heightmap;
		public ChunkData[,] chunks;

		public List<ChunkData> orphanChunks;

		public int[,,] finalBiomeData;

		public World ContainingWorld { get; internal set; }

		public RegionFileDataPositions RegionFileInfo { get; internal set; }

		public Region(int x, int z, World parentWorld, string sourceFilePath = null)
		{
			ContainingWorld = parentWorld;
			regionPos = new RegionLocation(x, z);
			chunks = new ChunkData[32, 32];
			this.sourceFilePath = sourceFilePath;
		}

		public Region(RegionLocation rloc, World parentWorld, string sourceFilePath = null) : this(rloc.x, rloc.z, parentWorld)
		{

		}

		///<summary>Returns true if the given locations contains air or the section has not been generated yet</summary>
		public bool IsAir(BlockCoord pos)
		{
			var b = GetBlock(pos);
			return b == null || b.IsAir;
		}

		///<summary>Is the location within the region's bounds?</summary>
		public bool IsWithinBoundaries(BlockCoord pos)
		{
			if (pos.x < 0 || pos.x >= 512 || pos.y < 0 || pos.y >= 256 || pos.z < 0 || pos.z >= 512) return false;
			else return true;
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(BlockCoord pos)
		{
			var b = GetBlock(pos);
			if (b == null) return false;
			return b == World.defaultBlock;
		}

		///<summary>Gets the block type at the given location.</summary>
		public ProtoBlock GetBlock(BlockCoord pos)
		{
			var chunk = GetChunk(pos.x, pos.z, false);
			if (chunk != null)
			{
				return chunk.GetBlockAt(pos.LocalChunkCoords)?.block ?? BlockList.Find("minecraft:air");
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(BlockCoord pos)
		{
			return GetChunk(pos.x, pos.z, false)?.GetBlockAt(pos.LocalChunkCoords);
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			return GetChunk(pos.x, pos.z, true)?.GetTileEntity(pos.LocalChunkCoords);
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, string block, bool allowNewChunks = false)
		{
			return SetBlock(pos, new BlockState(BlockList.Find(block)), allowNewChunks);
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockState block, bool allowNewChunks = false)
		{
			GetChunk(pos.x, pos.z, allowNewChunks)?.SetBlockAt(pos.LocalChunkCoords, block);
			return true;
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(BlockCoord pos, TileEntity te)
		{
			var chunk = GetChunk(pos.x, pos.z, true);
			chunk?.SetTileEntity(pos.LocalChunkCoords, te);
			return chunk != null;
		}

		/// <summary>
		/// Gets the chunk containing the block's position
		/// </summary>
		public ChunkData GetChunk(int localX, int localZ, bool allowNewChunks)
		{
			int chunkX = (int)Math.Floor(localX / 16.0);
			int chunkZ = (int)Math.Floor(localZ / 16.0);
			if (chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) throw new ArgumentOutOfRangeException();
			if (allowNewChunks && chunks[chunkX, chunkZ] == null)
			{
				chunks[chunkX, chunkZ] = new ChunkData(this, regionPos.GetChunkCoord(chunkX, chunkZ));
			}
			return chunks[chunkX, chunkZ];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(BlockCoord pos, bool allowNewChunks = false)
		{
			int chunkX = (int)Math.Floor(pos.x / 16.0);
			int chunkZ = (int)Math.Floor(pos.z / 16.0);
			if (chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return;
			if (chunks[chunkX, chunkZ] == null && allowNewChunks)
			{
				chunks[chunkX, chunkZ] = new ChunkData(this, regionPos.GetChunkCoord(chunkX, chunkZ));
			}
			var c = chunks[chunkX, chunkZ];
			if (c != null) c.SetDefaultBlockAt(pos.LocalChunkCoords);
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiomeAt(int x, int z)
		{
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				//for (int y = 0; y < 256; y++)
				//{
				return chunk.GetBiomeAt(x.Mod(16), z.Mod(16));
				//}
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiomeAt(BlockCoord pos)
		{
			var chunk = GetChunk(pos.x, pos.z, false);
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
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				chunk.SetBiomeAt(x.Mod(16), z.Mod(16), biome);
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiomeAt(BlockCoord pos, BiomeID biome)
		{
			var chunk = GetChunk(pos.x, pos.z, false);
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
			var chunk = GetChunk(pos.x, pos.z, false);
			if (chunk != null)
			{
				chunk.MarkForTickUpdate(pos.LocalChunkCoords);
			}
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when thie respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			var chunk = GetChunk(pos.x, pos.z, false);
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
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				return chunk.GetHighestBlock(x.Mod(16), z.Mod(16), heightmapType);
			}
			else
			{
				return short.MinValue;
			}
		}

		///<summary>Generates a heightmap by reading the chunk's heightmaps or calculating it from existing blocks.</summary>
		public short[,] GetHeightmapFromNBT(HeightmapType type)
		{
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

		public void WriteToFile(string destinationDirectory, GameVersion gameVersion, string name = null)
		{
			if (name == null) name = regionPos.ToFileName();
			using (var stream = new FileStream(Path.Combine(destinationDirectory, name), FileMode.Create))
			{
				RegionSerializer.WriteRegionToStream(this, stream, gameVersion);
			}
		}
	}
}
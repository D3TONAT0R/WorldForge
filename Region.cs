
using Ionic.Zlib;
using MCUtils.Coordinates;
using MCUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static MCUtils.ChunkData;
using static MCUtils.NBTContent;

namespace MCUtils {
	public class Region {

		public readonly RegionCoord regionPos;

		public byte[,] heightmap;
		public ChunkData[,] chunks;

		public List<ChunkData> orphanChunks;

		public int[,,] finalBiomeData;

		public World containingWorld;

		public Region(int x, int z) {
			regionPos = new RegionCoord(x, z);
			chunks = new ChunkData[32, 32];
		}

		public Region(World.RegionLocation rloc) : this(rloc.x, rloc.z) {

		}

		///<summary>Returns true if the given locations contains air or the section has not been generated yet</summary>
		public bool IsAir(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			return b == null || b.IsAir;
		}

		///<summary>Is the location within the region's bounds?</summary>
		public bool IsWithinBoundaries(int x, int y, int z) {
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return false;
			else return true;
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			if(b == null) return false;
			return b == World.defaultBlock;
		}

		///<summary>Gets the block type at the given location.</summary>
		public ProtoBlock GetBlock(int x, int y, int z) {
			var chunk = GetChunk(x, z, false);
			if(chunk != null) {
				return chunk.GetBlockAt(x % 16, y, z % 16)?.block ?? BlockList.Find("minecraft:air");
			} else {
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(int x, int y, int z) {
			return GetChunk(x,z,false)?.GetBlockAt(x % 16, y, z % 16);
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(int x, int y, int z)
		{
			return GetChunk(x, z, true)?.GetTileEntity(x % 16, y, z % 16);
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(int x, int y, int z, string block) {
			return SetBlock(x, y, z, new BlockState(BlockList.Find(block)));
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(int x, int y, int z, BlockState block) {
			GetChunk(x, z, true)?.SetBlockAt(x % 16, y, z % 16, block);
			return true;
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(int x, int y, int z, TileEntity te)
		{
			var chunk = GetChunk(x, z, true);
			chunk?.SetTileEntity(x % 16, y, z % 16, te);
			return chunk != null;
		}

		/// <summary>
		/// Gets the chunk containing the block's position
		/// </summary>
		public ChunkData GetChunk(int localX, int localZ, bool allowNew) {
			int chunkX = (int)Math.Floor(localX / 16.0);
			int chunkZ = (int)Math.Floor(localZ / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return null;
			if(allowNew && chunks[chunkX, chunkZ] == null) {
				chunks[chunkX, chunkZ] = new ChunkData(this, regionPos.GetChunkCoord(chunkX, chunkZ), "minecraft:stone");
			}
			return chunks[chunkX, chunkZ];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(int localX, int y, int localZ) {
			int chunkX = (int)Math.Floor(localX / 16.0);
			int chunkZ = (int)Math.Floor(localZ / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return;
			if(chunks[chunkX, chunkZ] == null) {
				chunks[chunkX, chunkZ] = new ChunkData(this, regionPos.GetChunkCoord(chunkX, chunkZ), "minecraft:stone");
			}
			chunks[chunkX, chunkZ].SetDefaultBlockAt(localX % 16, y, localZ % 16);
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiome(int x, int z)
		{
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				//for (int y = 0; y < 256; y++)
				//{
				return chunk.GetBiomeAt(x % 16, z % 16);
				//}
			} else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, BiomeID biome)
		{
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				//for(int y = 0; y < 256; y++)
				//{
				chunk.SetBiomeAt(x % 16, z % 16, biome);
				//}
			}
		}

		/// <summary>
		/// Marks the given coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(int x, int y, int z)
		{
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				chunk.MarkForTickUpdate(x % 16, y, z % 16);
			}
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when thie respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(int x, int y, int z)
		{
			var chunk = GetChunk(x, z, false);
			if (chunk != null)
			{
				chunk.UnmarkForTickUpdate(x % 16, y, z % 16);
			}
		}

		///<summary>Generates a heightmap by reading the chunk's heightmaps or calculating it from existing blocks.</summary>
		public short[,] GetHeightmapFromNBT(HeightmapType type) {
			short[,] hm = new short[512, 512];
			for(int x = 0; x < 32; x++) {
				for(int z = 0; z < 32; z++) {
					var c = chunks[x, z];
					if(c != null) {
						c.WriteToHeightmap(hm, x, z, type);
					}
				}
			}
			return hm;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks
		/// </summary>
		public int GetWaterDepth(int x, int y, int z)
		{
			int depth = 0;
			var block = GetBlock(x, y, z);
			while(block.IsWater)
			{
				depth++;
				y--;
				block = GetBlock(x, y, z);
			}
			return depth;
		}
	}
}
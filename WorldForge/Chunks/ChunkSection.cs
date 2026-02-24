using System;
using System.Collections.Generic;
using System.Linq;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using WorldForge.Lighting;

namespace WorldForge.Chunks
{
	public class ChunkSection
	{
		public readonly Chunk chunk;

		public ushort[] blocks;
		public List<BlockState> palette;

		//Resolution: [1:4:1] for backwards compatibility
		public byte[,,] biomes;
		public List<BiomeID> biomePalette;

		public LightValue[,,] lighting;

		private object lockObj = new object();

		public bool HasBlocksDefined => blocks != null && palette != null && palette.Count > 0;

		public bool HasBiomesDefined => biomes != null && biomePalette != null && biomePalette.Count > 0;

		public ChunkSection(Chunk chunk)
		{
			this.chunk = chunk;
			palette = new List<BlockState>
			{
				BlockState.Air
			};
		}

		#region Block related methods

		public void InitializeBlocks(BlockState block = null)
		{
			if (blocks != null && palette != null) throw new InvalidOperationException("Blocks have already been initialized for this section.");
			block = block ?? BlockState.Air;
			blocks = new ushort[16 * 16 * 16];
			palette = new List<BlockState>
			{
				block
			};
		}

		public BlockState GetBlock(int x, int y, int z)
		{
			if (!HasBlocksDefined) return BlockState.Air;
			return palette[blocks[GetArrayIndex(x, y, z)]];
		}

		public void SetBlock(int x, int y, int z, BlockState block)
		{
			lock (lockObj)
			{
				if (!HasBlocksDefined) InitializeBlocks();
				ushort index = GetPaletteIndex(block) ?? AddBlockToPalette(block);
				blocks[GetArrayIndex(x, y, z)] = index;
			}
		}

		public void SetBlock(int x, int y, int z, ushort paletteIndex)
		{
			lock (lockObj)
			{
				if (!HasBlocksDefined) InitializeBlocks();
				blocks[GetArrayIndex(x, y, z)] = paletteIndex;
			}
		}

		public void SetBlockColumn(int x, int z, int y1, int y2, BlockState block)
		{
			if (!HasBlocksDefined) InitializeBlocks();
			ushort index = GetPaletteIndex(block) ?? AddBlockToPalette(block);
			for (int y = y1; y <= y2; y++)
			{
				blocks[GetArrayIndex(x, y, z)] = index;
			}
		}

		public ushort? GetPaletteIndex(BlockState state)
		{
			if (!HasBlocksDefined) return null;
			for (ushort i = 0; i < palette.Count; i++)
			{
				if (palette[i].Compare(state)) return i;
			}
			return null;
		}

		private ushort AddBlockToPalette(BlockState block)
		{
			if (block == null) throw new NullReferenceException("Attempted to add a null BlockState to the palette.");
			palette.Add(block);
			return (ushort)(palette.Count - 1);
		}

		public static int GetArrayIndex(int x, int y, int z)
		{
			unchecked
			{
				return (z << 8) + (y << 4) + x;
			}
		}

		public static BlockCoord GetPositionFromArrayIndex(int i)
		{
			int x = i % 16;
			int y = (i / 16) % 16;
			int z = i / 256;
			return new BlockCoord(x, y, z);
		}

		public bool IsEmpty()
		{
			if (blocks == null) return true;
			foreach (var j in blocks)
			{
				int index = j;
				if (!palette[index].Compare(BlockState.Air)) return false;
			}
			return true;
		}

		#endregion

		#region Biome related methods

		public void InitializeBiomes(BiomeID biome = null)
		{
			if (biomes != null && biomePalette != null) throw new InvalidOperationException("Biomes have already been initialized for this section.");
			biome = biome ?? chunk.ParentDimension?.DefaultBiome ?? BiomeID.TheVoid;
			biomePalette = new List<BiomeID>();
			biomePalette.Add(biome);
			biomes = new byte[16, 4, 16];
		}

		public BiomeID GetBiome(int x, int y, int z)
		{
			if (!HasBiomesDefined) return null;
			var index = biomes[x & 0xF, (y / 4) & 3, z & 0xF];
			return biomePalette[index];
		}

		public BiomeID GetBiome(int x, int z)
		{
			return GetBiome(x, 15, z);
		}

		public byte? GetBiomePaletteIndex(BiomeID biome)
		{
			for (byte i = 0; i < biomePalette.Count; i++)
			{
				if (biomePalette[i].Compare(biome)) return i;
			}
			return null;
		}

		public BiomeID GetPredominantBiomeAt4x4(int x4, int y4, int z4)
		{
			Dictionary<byte, byte> indexCounts = new Dictionary<byte, byte>();
			if (!HasBiomesDefined) return chunk.ParentDimension?.DefaultBiome ?? BiomeID.TheVoid;
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int z1 = 0; z1 < 4; z1++)
				{
					var b = biomes[x4 * 4 + x1, y4, z4 * 4 + z1];
					if (!indexCounts.ContainsKey(b))
					{
						indexCounts.Add(b, 0);
					}
					indexCounts[b]++;
				}
			}
			BiomeID biome = BiomeID.TheVoid;
			int maxCells = 0;
			foreach (var k in indexCounts.Keys)
			{
				if (indexCounts[k] > maxCells)
				{
					maxCells = indexCounts[k];
					biome = biomePalette[k];
				}
			}
			return biome;
		}

		public void SetBiome(int x, int y, int z, BiomeID biome)
		{
			//NOTE: biomes have a vertical resolution of 4 blocks
			if (biomes == null) InitializeBiomes();
			x &= 0xF;
			y &= 0xF;
			z &= 0xF;
			lock (lockObj)
			{
				var index = GetBiomePaletteIndex(biome);
				if (index.HasValue)
				{
					biomes[x, y / 4, z] = index.Value;
				}
				else
				{
					biomePalette.Add(biome);
					biomes[x, y / 4, z] = (byte)(biomePalette.Count - 1);
				}
			}
		}

		public void SetBiome3D4x4(int x, int y, int z, BiomeID biome)
		{
			for (int x1 = x / 4; x1 < x / 4 + 1; x1++)
			{
				for (int z1 = z / 4; z1 < z / 4 + 1; z1++)
				{
					SetBiome(x1, y / 4, z1, biome);
				}
			}
		}

		public void SetBiomeColumn(int x, int z, BiomeID biome)
		{
			for (int y = 0; y < 16; y += 4)
			{
				SetBiome(x, y, z, biome);
			}
		}

		#endregion

		#region Lighting related methods

		public LightValue GetLight(BlockCoord pos)
		{
			if (lighting == null) return LightValue.None;
			return lighting[pos.x, pos.y, pos.z];
		}

		public void SetLight(BlockCoord pos, LightValue value)
		{
			lock (lockObj)
			{
				if (lighting == null) lighting = new LightValue[16, 16, 16];
				lighting[pos.x, pos.y, pos.z] = value;
			}
		}

		#endregion

		public ChunkSection Clone()
		{
			var clone = new ChunkSection(chunk);
			if (blocks != null) clone.blocks = (ushort[])blocks.Clone();
			if (palette != null) clone.palette = new List<BlockState>(palette);
			if (biomes != null) clone.biomes = (byte[,,])biomes.Clone();
			if (biomePalette != null) clone.biomePalette = new List<BiomeID>(biomePalette);
			if (lighting != null) clone.lighting = (LightValue[,,])lighting.Clone();
			return clone;
		}
	}
}

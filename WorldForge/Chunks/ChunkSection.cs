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

		public ushort[] blocks = new ushort[4096];
		public List<BlockState> palette;

		//Resolution: [1:4:1] for backwards compatibility
		private BiomeID[,,] biomes;

		public LightValue[,,] lighting;

		public bool HasBiomesDefined => biomes != null;

		public ChunkSection(Chunk chunk)
		{
			this.chunk = chunk;
			palette = new List<BlockState>
			{
				BlockState.Air
			};
		}

		#region Block related methods

		public BlockState GetBlock(int x, int y, int z)
		{
			return palette[blocks[GetArrayIndex(x, y, z)]];
		}

		public void SetBlock(int x, int y, int z, BlockState block)
		{
			ushort? index = GetPaletteIndex(block);
			if(index == null)
			{
				index = AddBlockToPalette(block);
			}
			blocks[GetArrayIndex(x, y, z)] = (ushort)index;
		}

		public void SetBlock(int x, int y, int z, ushort paletteIndex)
		{
			blocks[GetArrayIndex(x, y, z)] = paletteIndex;
		}

		public ushort? GetPaletteIndex(BlockState state)
		{
			for(short i = 0; i < palette.Count; i++)
			{
				if(palette[i].Compare(state)) return (ushort)i;
			}
			return null;
		}

		private ushort AddBlockToPalette(BlockState block)
		{
			if(block == null) throw new NullReferenceException("Attempted to add a null BlockState to the palette.");
			palette.Add(block);
			return (ushort)(palette.Count - 1);
		}

		public static int GetArrayIndex(int x, int y, int z)
		{
			return z * 256 + y * 16 + x;
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
			if(blocks == null) return true;
			foreach(var j in blocks)
			{
				int index = j;
				if(!palette[index].Compare(BlockState.Air)) return false;
			}
			return true;
		}

		#endregion

		#region Biome related methods

		public void InitializeBiomes()
		{
			BiomeID fallback = chunk.ParentDimension?.DefaultBiome ?? BiomeID.TheVoid;
			var lowest = chunk.LowestSection;
			sbyte secY = chunk.Sections.First(kv => kv.Value == this).Key;
			ChunkSection belowSection = null;
			while(secY > lowest)
			{
				secY--;
				if(chunk.Sections.TryGetValue(secY, out var sec))
				{
					if(sec.HasBiomesDefined)
					{
						belowSection = sec;
						break;
					}
				}
			}
			biomes = new BiomeID[16, 4, 16];
			for(int x = 0; x < 16; x++)
			{
				for(int z = 0; z < 16; z++)
				{
					BiomeID b = belowSection?.GetBiome(x, z) ?? fallback;
					for(int y = 0; y < 4; y++)
					{
						biomes[x, y, z] = b;
					}
				}
			}
		}

		public BiomeID GetBiome(int x, int y, int z)
		{
			if(biomes == null) return null;
			return biomes[x & 0xF, (y / 4) & 3, z & 0xF];
		}

		public BiomeID GetBiome(int x, int z)
		{
			return GetBiome(x, 15, z);
		}

		public BiomeID GetPredominantBiomeAt4x4(int x4, int y4, int z4)
		{
			Dictionary<BiomeID, byte> occurences = new Dictionary<BiomeID, byte>();
			if(!HasBiomesDefined) return chunk.ParentDimension?.DefaultBiome ?? BiomeID.TheVoid;
			for(int x1 = 0; x1 < 4; x1++)
			{
				for(int z1 = 0; z1 < 4; z1++)
				{
					var b = biomes[x4 * 4 + x1, y4, z4 * 4 + z1];
					if(!occurences.ContainsKey(b))
					{
						occurences.Add(b, 0);
					}
					occurences[b]++;
				}
			}
			BiomeID predominantBiome = BiomeID.Ocean;
			int predominantCells = 0;
			foreach(var k in occurences.Keys)
			{
				if(occurences[k] > predominantCells)
				{
					predominantCells = occurences[k];
					predominantBiome = k;
				}
			}
			return predominantBiome;
		}

		public void SetBiome(int x, int y, int z, BiomeID biome)
		{
			//NOTE: biomes have a vertical resolution of 4 blocks
			if(biomes == null) InitializeBiomes();
			x &= 0xF;
			y &= 0xF;
			z &= 0xF;
			biomes[x, y / 4, z] = biome;
		}

		public void SetBiome3D4x4(int x, int y, int z, BiomeID biome)
		{
			for(int x1 = x / 4; x1 < x / 4 + 1; x1++)
			{
				for(int z1 = z / 4; z1 < z / 4 + 1; z1++)
				{
					SetBiome(x1, y / 4, z1, biome);
				}
			}
		}

		public void SetBiomeColumn(int x, int z, BiomeID biome)
		{
			for(int y = 0; y < 16; y += 4)
			{
				SetBiome(x, y, z, biome);
			}
		}

		#endregion

		#region Lighting related methods

		public LightValue GetLight(BlockCoord pos)
		{
			if(lighting == null) return LightValue.None;
			return lighting[pos.x, pos.y, pos.z];
		}

		public void SetLight(BlockCoord pos, LightValue value)
		{
			if(lighting == null) lighting = new LightValue[16, 16, 16];
			lighting[pos.x, pos.y, pos.z] = value;
		}

		#endregion
	}
}

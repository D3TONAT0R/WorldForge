using MCUtils.Lighting;
using MCUtils.NBT;
using System;
using System.Collections.Generic;

namespace MCUtils
{
	public class ChunkSection
	{
		public ushort[,,] blocks = new ushort[16, 16, 16];
		public List<BlockState> palette;

		//Resolution: [1:4:1] for backwards compatibility
		public BiomeID[,,] biomes;

		public LightValue[,,] lightmap;

		public bool HasBiomesDefined => biomes != null;

		//private readonly object lockObj = new object();

		public ChunkSection(string defaultBlock)
		{
			palette = new List<BlockState>();
			if (defaultBlock != null)
			{
				palette.Add(new BlockState(BlockList.Find("minecraft:air"))); //Index 0
				palette.Add(new BlockState(BlockList.Find(defaultBlock))); //Index 1
			}
		}

		public void SetBlockAt(int x, int y, int z, BlockState block)
		{
			//lock (lockObj)
			//{
			ushort? index = GetPaletteIndex(block);
			if (index == null)
			{
				index = AddBlockToPalette(block);
			}
			blocks[x, y, z] = (ushort)index;
			//}
		}

		public void SetBlockAt(int x, int y, int z, ushort paletteIndex)
		{
			//lock (lockObj)
			//{
			blocks[x, y, z] = paletteIndex;
			//}
		}

		public BlockState GetBlockAt(int x, int y, int z)
		{
			return palette[blocks[x, y, z]];
		}

		public ushort? GetPaletteIndex(BlockState state)
		{
			for (short i = 0; i < palette.Count; i++)
			{
				if (palette[i].Compare(state, true)) return (ushort)i;
			}
			return null;
		}

		private ushort AddBlockToPalette(BlockState block)
		{
			if (block == null) throw new NullReferenceException("Attempted to add a null BlockState to the palette.");
			palette.Add(block);
			return (ushort)(palette.Count - 1);
		}

		public bool IsEmpty()
		{
			if (blocks == null) return true;
			bool allSame = true;
			var i = blocks[0, 0, 0];
			if (!palette[i].Compare(BlockState.Air, false)) return false;
			foreach (var j in blocks)
			{
				allSame &= i == j;
			}
			return allSame;
		}

		public void SetBiomeAt(int x, int y, int z, BiomeID biome)
		{
			//NOTE: biomes have a vertical resolution of 4 blocks
			if (biomes == null) biomes = new BiomeID[16, 4, 16];
			x = x.Mod(16);
			z = z.Mod(16);
			y = y.Mod(16);
			biomes[x, y / 4, z] = biome;
		}

		public void SetBiome3D4x4At(int x, int y, int z, BiomeID biome)
		{
			for (int x1 = x / 4; x1 < x / 4 + 1; x1++)
			{
				for (int z1 = z / 4; z1 < z / 4 + 1; z1++)
				{
					SetBiomeAt(x1, y / 4, z1, biome);
				}
			}
		}

		public void SetBiomeColumnAt(int x, int z, BiomeID biome)
		{
			for (int y = 0; y < 16; y += 4)
			{
				SetBiomeAt(x, y, z, biome);
			}
		}

		public BiomeID GetBiomeAt(int x, int y, int z)
		{
			if (biomes == null) return BiomeID.plains;
			return biomes[x.Mod(16), (y / 4).Mod(4), z.Mod(16)];
		}

		public BiomeID GetBiomeAt(int x, int z)
		{
			return GetBiomeAt(x, 15, z);
		}


		public BiomeID GetPredominantBiomeAt4x4(int x4, int y4, int z4)
		{
			Dictionary<BiomeID, byte> occurences = new Dictionary<BiomeID, byte>();
			if (!HasBiomesDefined) return BiomeID.plains;
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int z1 = 0; z1 < 4; z1++)
				{
					var b = biomes[x4 + x1, y4, z4 + z1];
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

		public LightValue GetLightAt(int x, int y, int z)
		{
			if (lightmap == null) return LightValue.None;
			return lightmap[x, y, z];
		}

		public void SetLightAt(int x, int y, int z, LightValue value)
		{
			if (lightmap == null) lightmap = new LightValue[16,16,16];
			lightmap[x, y, z] = value;
		}
	}
}

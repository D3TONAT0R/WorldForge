using System;
using System.Collections.Generic;
using WorldForge.Chunks;
using WorldForge.Coordinates;

namespace WorldForge.Lighting
{
	public class Lightmapper
	{

		public void BakeChunkLight(ChunkData chunk)
		{
			BakeSkyLight(chunk);
		}

		private static int LMByteIndex(int x, int y, int z)
		{
			return (y * 256 + z * 16 + x) / 2;
		}

		public static void EncodeLightmap(LightValue[,,] sectionLM, out byte[] blockLight, out byte[] skyLight)
		{
			blockLight = new byte[sectionLM.Length / 2];
			skyLight = new byte[sectionLM.Length / 2];
			for(int y = 0; y < sectionLM.GetLength(1); y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x += 2)
					{
						int i = LMByteIndex(x, y, z);
						blockLight[i] = BitUtils.CompressNibbles(sectionLM[x, y, z].BlockLight, sectionLM[x + 1, y, z].BlockLight);
						skyLight[i] = BitUtils.CompressNibbles(sectionLM[x, y, z].SkyLight, sectionLM[x + 1, y, z].SkyLight);
					}
				}
			}
		}

		public static LightValue[,,] DecodeLightmap(byte[] blockLight, byte[] skyLight)
		{
			if(blockLight.Length != skyLight.Length) throw new ArgumentException("BlockLight and SkyLight array lengths do not match.");
			var lm = new LightValue[16, blockLight.Length / 128, 16];
			for(int y = 0; y < 16; y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x += 2)
					{
						int i = LMByteIndex(x, y, z);
						BitUtils.ExtractNibblesFromByte(blockLight[i], out var blLow, out var blHigh);
						BitUtils.ExtractNibblesFromByte(skyLight[i], out var slLow, out var slHigh);
						lm[x, y, z] = new LightValue(blLow, slLow);
						lm[x + 1, y, z] = new LightValue(blHigh, slHigh);
					}
				}
			}
			return lm;
		}

		private void BakeSkyLight(ChunkData chunk)
		{
			var maxY = chunk.HighestSection * 16 + 15;
			//var hs = chunk.sections[chunk.HighestSection];
			for(int z = 0; z < 16; z++)
			{
				for(int x = 0; x < 16; x++)
				{
					//HACK: light starts spreading only at the first surface block
					int y = chunk.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
					//int y = maxY;
					LightValue lv = ApplyOcclusion(LightValue.FullSkyLight, GetOcclusionLevelAtLocationChunkOnly(chunk, (x, y, z)));
					Spread((x, y, z), lv, chunk);
				}
			}
		}

		private void BakeBlockLight(ChunkData chunk)
		{
			List<(BlockCoord pos, byte l)> lightSources = new List<(BlockCoord pos, byte l)>();
			foreach(var sY in chunk.Sections.Keys)
			{
				var s = chunk.Sections[sY];
				for(int y = 0; y < 16; y++)
				{
					for(int z = 0; z < 16; z++)
					{
						for(int x = 0; x < 16; x++)
						{

						}
					}
				}
			}
		}

		private void SpreadHorizontal(BlockCoord pos, LightValue l, ChunkData limitChunk)
		{
			if(l.IsDark) return;
			LightValue oxn = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Left)).Attenuated;
			TrySpreadTo(pos.Left, limitChunk, oxn);
			LightValue oxp = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Right)).Attenuated;
			TrySpreadTo(pos.Right, limitChunk, oxp);
			LightValue ozn = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Back)).Attenuated;
			TrySpreadTo(pos.Back, limitChunk, ozn);
			LightValue ozp = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Forward)).Attenuated;
			TrySpreadTo(pos.Forward, limitChunk, ozp);
		}

		private void Spread(BlockCoord pos, LightValue l, ChunkData limitChunk)
		{
			if(l.IsDark) return;
			SpreadHorizontal(pos, l, limitChunk);
			LightValue oyn = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Below)).AttenuatedDown;
			TrySpreadTo(pos.Below, limitChunk, oyn);
			LightValue oyp = ApplyOcclusion(l, GetOcclusionLevelAtLocationChunkOnly(limitChunk, pos.Above)).Attenuated;
			TrySpreadTo(pos.Above, limitChunk, oyp);
		}

		private void TrySpreadTo(BlockCoord pos, ChunkData limitChunk, LightValue l)
		{
			if(pos.x < 0 || pos.x > 15 || pos.z < 0 || pos.z > 15 || pos.y < limitChunk.LowestSection * 16 || pos.y > limitChunk.HighestSection * 16 + 15) return;
			var existingLight = limitChunk.GetChunkSectionForYCoord(pos.y, false)?.GetLightAt(pos.LocalSectionCoords) ?? LightValue.FullBright;
			if(!l.IsDark && l.HasStrongerLightThan(existingLight))
			{
				limitChunk.GetChunkSectionForYCoord(pos.y, false)?.SetLightAt(pos.LocalSectionCoords, l);
				Spread(pos, l, limitChunk);
			}
		}

		public byte GetOcclusionLevel(BlockID block)
		{
			if(block == null || block.IsAir) return 0;
			else if(block.IsLiquid) return 1;
			else if(Blocks.IsTransparentBlock(block)) return 0;
			else return 15;
		}

		public byte GetOcclusionLevelAtLocationChunkOnly(ChunkData c, BlockCoord pos)
		{
			if(pos.x < 0 || pos.x > 15 || pos.z < 0 || pos.z > 15 || pos.y < c.LowestSection * 16 || pos.y > c.HighestSection * 16 + 15)
			{
				return 15;
			}
			else
			{
				return GetOcclusionLevel(c.GetBlockAt(pos)?.Block);
			}
		}

		public LightValue ApplyOcclusion(LightValue l, byte occ)
		{
			if(occ > 14)
			{
				return LightValue.None;
			}
			else if(occ == 0)
			{
				return l;
			}
			else
			{
				l.SkyLight -= occ;
				return l;
			}
		}
	}
}

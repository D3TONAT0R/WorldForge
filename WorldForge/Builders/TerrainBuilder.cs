using System;
using WorldForge.Coordinates;

namespace WorldForge.Builders
{
	public class TerrainBuilder
	{

		public enum BedrockType
		{
			None,
			Flat,
			Noisy
		}

		public bool CreateNewRegions { get; set; } = true;

		public BlockID FillBlock { get; set; }

		public int BottomHeight { get; set; } = 0;

		public BedrockType BedrockPattern { get; set; } = BedrockType.Noisy;


		public TerrainBuilder(BlockID fillBlock = null)
		{
			FillBlock = fillBlock ?? BlockList.Find("stone");
		}

		public void FillBlockColumn(Dimension dim, int x, int z, int height)
		{
			var pos = new BlockCoord(x, height, z);
			if(CreateNewRegions && !dim.TryGetRegionAtBlock(x, z, out _))
			{
				dim.CreateRegionIfMissing(pos.Region);
			}
			for(int y = 0; y <= height; y++)
			{
				SetBlock(dim, new BlockCoord(x, y, z), height);
			}
		}

		protected virtual void SetBlock(Dimension dim, BlockCoord coord, int height)
		{
			dim.SetBlock(coord, FillBlock, true);
		}

		protected bool TryCreateBedrock(Dimension dim, BlockCoord coord, long seed)
		{
			if(coord.y == BottomHeight && BedrockPattern == BedrockType.Flat
				|| coord.y < BottomHeight + 4 && BedrockPattern == BedrockType.Noisy && SeededRandom.ProbabilityInt(coord.y + 1 - BottomHeight, seed, coord))
			{
				dim.SetBlock(coord, BlockList.Find("bedrock"), true);
				return true;
			}
			return false;
		}
	}
}
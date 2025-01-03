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

		private readonly Random random = new Random();

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
			if(CreateNewRegions && !dim.TryGetRegionAtBlock(x, z, out _))
			{
				dim.CreateRegionIfMissing(x.Mod(512), z.Mod(512));
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

		protected bool TryCreateBedrock(Dimension dim, BlockCoord coord)
		{
			if(coord.y == BottomHeight && BedrockPattern == BedrockType.Flat
				|| coord.y < BottomHeight + 4 && BedrockPattern == BedrockType.Noisy && ProbabilityInt(coord.y + 1 - BottomHeight))
			{
				dim.SetBlock(coord, BlockList.Find("bedrock"), true);
				return true;
			}
			return false;
		}

		protected bool Probability(float prob)
		{
			return random.NextDouble() <= prob;
		}

		protected bool ProbabilityInt(int i)
		{
			return random.Next(i) == 0;
		}
	}
}
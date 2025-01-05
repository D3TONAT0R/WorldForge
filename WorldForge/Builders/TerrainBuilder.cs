using System;
using WorldForge.Coordinates;
using static WorldForge.Builders.OverworldTerrainBuilder;

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

		private readonly BlockState bedrock = new BlockState("bedrock");

		public bool CreateNewRegions { get; set; } = true;

		public BlockState FillBlock { get; set; }

		public int BottomHeight { get; set; } = 0;

		public long Seed { get; set; } = -1;

		public BedrockType BedrockPattern { get; set; } = BedrockType.Noisy;

		public TerrainBuilder(BlockState fillBlock = null)
		{
			FillBlock = fillBlock ?? new BlockState("stone");
		}

		public void FillBlockColumn(Dimension dim, int x, int z, int height)
		{
			var pos = new BlockCoord(x, height, z);
			if(CreateNewRegions && !dim.TryGetRegionAtBlock(pos, out _))
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
			var seed = Seed == -1 ? (dim.ParentWorld?.LevelData.worldGen.WorldSeed ?? 0) : Seed;
			if(!TryCreateBedrock(dim, coord, seed))
			{
				var block = FillBlock;
				dim.SetBlock(coord, block, true);
			}
		}

		protected bool TryCreateBedrock(Dimension dim, BlockCoord coord, long seed)
		{
			if(coord.y == BottomHeight && BedrockPattern == BedrockType.Flat
				|| coord.y < BottomHeight + 4 && BedrockPattern == BedrockType.Noisy && SeededRandom.ProbabilityInt(coord.y + 1 - BottomHeight, seed, coord))
			{
				dim.SetBlock(coord, bedrock, true);
				return true;
			}
			return false;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.Coordinates;

namespace WorldForge.Builders
{
	public class OverworldTerrainBuilder
	{
		public enum SurfacePreset
		{
			None,
			Grass,
			Sand,
			Gravel
		}

		public enum BedrockType
		{
			None,
			Flat,
			Noisy
		}

		public Dimension TargetDimension { get; set; }

		public BedrockType BedrockPattern { get; set; } = BedrockType.Noisy;

		private readonly Random random = new Random();

		private BlockID stone = BlockList.Find("stone");
		private BlockID bedrock = BlockList.Find("bedrock");
		private BlockID grass = BlockList.Find("grass_block");
		private BlockID dirt = BlockList.Find("dirt");
		private BlockID sand = BlockList.Find("sand");
		private BlockID gravel = BlockList.Find("gravel");

		public OverworldTerrainBuilder(Dimension targetDimension)
		{
			TargetDimension = targetDimension;
		}

		public void FillBlockColumn(int x, int z, int height, SurfacePreset preset)
		{
			for(int y = 0; y <= height; y++) {
				BlockID block = stone;
				if(y == 0 && BedrockPattern == BedrockType.Flat) block = bedrock;
				else if(y < 4 && BedrockPattern == BedrockType.Noisy && ProbabilityInt(y + 1)) block = bedrock;
				else if(preset != SurfacePreset.None)
                {
                    if(y == height) block = GetBlockForPreset(preset, true);
					else if(y > height - 3) block = GetBlockForPreset(preset, false);
					else if(y == height - 3 && Probability(0.5f)) block = GetBlockForPreset(preset, false);
				}
                TargetDimension.SetBlock(new BlockCoord(x, y, z), block);
			}
		}

		private BlockID GetBlockForPreset(SurfacePreset preset, bool top)
		{
			switch(preset)
			{
				case SurfacePreset.Grass: return top ? grass : dirt;
				case SurfacePreset.Sand: return sand;
				case SurfacePreset.Gravel: return gravel;
				default: return stone;
			}
		}

		private bool Probability(float prob)
		{
			return random.NextDouble() <= prob;
		}

		private bool ProbabilityInt(int i)
		{
			return random.Next(i) == 0;
		}
	}
}

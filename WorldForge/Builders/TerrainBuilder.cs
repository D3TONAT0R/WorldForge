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

		public enum BedrockPreset
		{
			None,
			Flat,
			Noisy
		}

		public Dimension TargetDimension { get; set; }

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

		public void FillBlockColumn(int x, int z, int height, SurfacePreset preset, BedrockPreset bedrockPreset = BedrockPreset.Noisy)
		{
			for(int y = 0; y <= height; y++) {
				BlockID block = stone;
				if(y == 0 && bedrockPreset == BedrockPreset.Flat) block = bedrock;
				else if(y < 4 && bedrockPreset == BedrockPreset.Noisy && random.Next(y + 1) == 0) block = bedrock;
				else if(preset != SurfacePreset.None)
                {
                    if(y == height) block = GetBlockForPreset(preset, true);
					else if(y > height - 3) block = GetBlockForPreset(preset, false);
					else if(y == height - 3 && random.Next(2) == 0) block = GetBlockForPreset(preset, false);
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
	}
}

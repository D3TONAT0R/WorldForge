using WorldForge.Coordinates;

namespace WorldForge.Builders
{
	public class OverworldTerrainBuilder : TerrainBuilder
	{
		public enum SurfacePreset
		{
			None,
			Grass,
			Sand,
			Gravel
		}

		private BlockID bedrock = BlockList.Find("bedrock");
		private BlockID grass = BlockList.Find("grass_block");
		private BlockID dirt = BlockList.Find("dirt");
		private BlockID sand = BlockList.Find("sand");
		private BlockID gravel = BlockList.Find("gravel");

		public SurfacePreset Surface { get; set; }

		public float SurfaceLayerThickness { get; set; } = 3.5f;

		public OverworldTerrainBuilder(SurfacePreset surface)
		{
			Surface = surface;
		}

		protected override void SetBlock(Dimension dim, BlockCoord coord, int height)
		{
			if(!TryCreateBedrock(dim, coord))
			{
				BlockID block = FillBlock;
				if(Surface != SurfacePreset.None)
				{
					int t = (int)SurfaceLayerThickness;
					if(coord.y == height) block = GetBlockForPreset(Surface, true);
					else if(coord.y > height - t) block = GetBlockForPreset(Surface, false);
					else if(coord.y == height - t && Probability(SurfaceLayerThickness - t)) block = GetBlockForPreset(Surface, false);
				}
				dim.SetBlock(coord, block, true);
			}
		}

		private BlockID GetBlockForPreset(SurfacePreset preset, bool top)
		{
			switch(preset)
			{
				case SurfacePreset.Grass: return top ? grass : dirt;
				case SurfacePreset.Sand: return sand;
				case SurfacePreset.Gravel: return gravel;
				default: return FillBlock;
			}
		}
	}
}

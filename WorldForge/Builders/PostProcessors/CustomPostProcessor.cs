using System;
using WorldForge.Coordinates;
using WorldForge.Regions;

namespace WorldForge.Builders.PostProcessors
{
	public class CustomPostProcessor : PostProcessor
	{
		private readonly Action<Dimension, BlockCoord> surfaceProcessor;
		private readonly Action<Dimension, BlockCoord> blockProcessor;
		private readonly Action<Region> regionProcessor;

		public bool multiThreaded;

		public override PostProcessType PostProcessorType
		{
			get
			{
				if(surfaceProcessor != null && blockProcessor != null)
				{
					return PostProcessType.Both;
				}
				if(surfaceProcessor != null)
				{
					return PostProcessType.Surface;
				}
				if(blockProcessor != null)
				{
					return PostProcessType.Block;
				}
				return PostProcessType.RegionOnly;
			}
		}

		public override bool Multithreading => multiThreaded;

		public CustomPostProcessor(Action<Dimension, BlockCoord> surfaceProcessor, Action<Dimension, BlockCoord> blockProcessor, Action<Region> regionProcessor)
		{
			this.surfaceProcessor = surfaceProcessor;
			this.blockProcessor = blockProcessor;
			this.regionProcessor = regionProcessor;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			surfaceProcessor.Invoke(dimension, pos);
		}

		protected override void OnProcessBlock(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			blockProcessor.Invoke(dimension, pos);
		}

		protected override void OnProcessRegion(Region reg, int pass)
		{
			regionProcessor?.Invoke(reg);
		}
	}
}
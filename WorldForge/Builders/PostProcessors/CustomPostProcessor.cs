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
				var type = PostProcessType.None;
				if(surfaceProcessor != null)
				{
					type |= PostProcessType.Surface;
				}
				if(blockProcessor != null)
				{
					type |= PostProcessType.Block;
				}
				return type;
			}
		}

		public override bool UseMultithreading => multiThreaded;

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
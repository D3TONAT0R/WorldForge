using System;
using WorldForge;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public abstract class Layer
	{
		public abstract void ProcessBlockColumn(Dimension dim, Random random, BlockCoord pos, float mask);
	}
}

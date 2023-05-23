using System;
using System.Threading.Tasks;

namespace MCUtils.Utilities.BlockDistributionAnalysis
{
	public class Analyzer
	{

		public AnalysisData analysisData = new AnalysisData();

		public short YMin { get; private set; }
		public short YMax { get; private set; }

		public Analyzer(short yMin = -64, short yMax = 320)
		{
			YMin = yMin;
			YMax = yMax;
		}

		public void AnalyzeChunk(ChunkData chunk)
		{
			try
			{
				if(chunk.HasFullyGeneratedTerrain)
				{
					chunk.ForEachBlock(YMin, YMax, (pos, b) =>
					{
						if(b != null)
						{
							analysisData.IncreaseCounter(b.block, (short)pos.y);
						}
					});
					analysisData.chunkCounter++;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"Failed to read chunk at {chunk.worldSpaceCoord}: " + e.Message);
			}
		}

		public void AnalyzeRegion(Region region, bool parallel = true)
		{
			if(parallel)
			{
				Parallel.For(0, 1024, (i) =>
				{
					var chunk = region.chunks[i % 32, i / 32];
					if(chunk != null)
					{
						AnalyzeChunk(chunk);
					}
				});
			}
			else
			{
				for(int i = 0; i < 1024; i++)
				{
					var chunk = region.chunks[i % 32, i / 32];
					if(chunk != null)
					{
						AnalyzeChunk(chunk);
					}
				}
			}
		}
	}
}

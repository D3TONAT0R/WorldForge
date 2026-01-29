using System;
using System.Threading;
using System.Threading.Tasks;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.Regions;

namespace WorldForge.Utilities.BlockDistributionAnalysis
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

		public void AnalyzeChunk(Chunk chunk)
		{
			try
			{
				if (chunk.HasFullyGeneratedTerrain)
				{
					chunk.ForEachBlock(YMin, YMax, (pos, b) =>
					{
						if (b != null)
						{
							analysisData.IncreaseCounter(b.Block, (short)pos.y);
						}
					});
					analysisData.chunkCounter++;
				}
			}
			catch (Exception e)
			{
				Logger.Warning($"Failed to read chunk at {chunk.WorldSpaceCoord}: " + e.Message);
			}
		}

		public void AnalyzeRegion(Region region, bool parallel = true, CancellationToken? token = null)
		{
			if (parallel)
			{
				var options = new ParallelOptions { CancellationToken = token ?? CancellationToken.None };
				Parallel.For(0, 1024, options, (i) =>
				{
					var chunk = region.chunks[i % 32, i / 32];
					if (IsChunkReady(chunk)) AnalyzeChunk(chunk);
				});
			}
			else
			{
				for (int i = 0; i < 1024; i++)
				{
					if (token?.IsCancellationRequested ?? false) break;
					var chunk = region.chunks[i % 32, i / 32];
					if (IsChunkReady(chunk)) AnalyzeChunk(chunk);
				}
			}
		}

		public void AnalyzeDimension(Dimension dimension, bool parallel = true, CancellationToken? token = null, IProgress<int> progress = null)
		{
			object lockObj = new object();
			int processedRegions = 0;
			if (parallel)
			{
				var options = new ParallelOptions { CancellationToken = token ?? CancellationToken.None };
				try
				{
					Parallel.ForEach(dimension.regions, options, kv =>
					{
						var region = kv.Value;
						if (!region.IsLoaded) region = region.LoadClone();
						AnalyzeRegion(region, false, token);
						lock (lockObj)
						{
							processedRegions++;
							progress?.Report(processedRegions);
						}
					});
				}
				catch (OperationCanceledException)
				{
					// Ignore cancellation
				}
			}
			else
			{
				foreach (var region in dimension.regions.Values)
				{
					if (token?.IsCancellationRequested ?? false) break;
					var loadedRegion = region;
					if (!region.IsLoaded) loadedRegion = region.LoadClone();
					AnalyzeRegion(loadedRegion, false);
					lock (lockObj)
					{
						processedRegions++;
						progress?.Report(processedRegions);
					}
				}
			}
		}

		public void AnalyzeDimensionArea(Dimension dimension, ChunkCoord scanOriginChunk, int browserScanChunkRadius, bool parallel = true, CancellationToken? token = null, IProgress<int> progress = null)
		{
			object lockObj = new object();
			int processedChunks = 0;
			var minChunk = new ChunkCoord(scanOriginChunk.x - browserScanChunkRadius, scanOriginChunk.z - browserScanChunkRadius);
			var maxChunk = new ChunkCoord(scanOriginChunk.x + browserScanChunkRadius, scanOriginChunk.z + browserScanChunkRadius);
			if (parallel)
			{
				var options = new ParallelOptions { CancellationToken = token ?? CancellationToken.None };
				try
				{
					Parallel.ForEach(dimension.EnumerateChunks(minChunk, maxChunk, false, true), options, chunk =>
					{
						if (IsChunkReady(chunk)) AnalyzeChunk(chunk);
						lock (lockObj)
						{
							processedChunks++;
							progress?.Report(processedChunks);
						}
					});
				}
				catch (OperationCanceledException)
				{
					// Ignore cancellation
				}
			}
			else
			{
				foreach (var chunk in dimension.EnumerateChunks(minChunk, maxChunk, false, true))
				{
					if (IsChunkReady(chunk)) AnalyzeChunk(chunk);
					lock (lockObj)
					{
						processedChunks++;
						progress?.Report(processedChunks);
					}
				}
			}
		}

		private bool IsChunkReady(Chunk chunk)
		{
			return chunk != null && chunk.HasFullyGeneratedTerrain;
		}
	}
}

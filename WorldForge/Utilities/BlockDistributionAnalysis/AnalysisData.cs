using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WorldForge.Utilities.BlockDistributionAnalysis
{
	[Serializable]
	public class AnalysisData
	{
		[Serializable]
		public class BlockCount
		{
			public Dictionary<short, long> counts = new Dictionary<short, long>();

			public void Increase(short y)
			{
				if(!counts.ContainsKey(y)) counts.Add(y, 0);
				counts[y]++;
			}

			public long GetCountAtY(short y)
			{
				if(counts.TryGetValue(y, out var c))
				{
					return c;
				}
				else
				{
					return 0;
				}
			}

			public double GetRateAtY(short y, int chunkCount)
			{
				return GetCountAtY(y) / (double)chunkCount / 256d;
			}
		}

		public Dictionary<NamespacedID, BlockCount> data = new Dictionary<NamespacedID, BlockCount>();
		public int chunkCounter;

		private readonly object lockObj = new object();

		public void IncreaseCounter(NamespacedID blockID, short y)
		{
			lock(lockObj)
			{
				if(!data.ContainsKey(blockID))
				{
					data.Add(blockID, new BlockCount());
				}
				data[blockID].Increase(y);
			}
		}

		public void IncreaseCounter(BlockID block, short y)
		{
			if(block != null) IncreaseCounter(block.ID, y);
		}

		public long GetTotalAtY(NamespacedID blockID, short y)
		{
			if(data.TryGetValue(blockID, out var c))
			{
				return c.GetCountAtY(y);
			}
			else
			{
				return 0;
			}
		}

		public double GetRateAtY(NamespacedID blockID, short y)
		{
			if(data.TryGetValue(blockID, out var c))
			{
				return c.GetRateAtY(y, chunkCounter);
			}
			else
			{
				return 0.0;
			}
		}


		public void SaveToFile(string path)
		{
			string json = JsonConvert.SerializeObject(this);
			File.WriteAllText(path, json);
		}

		public static AnalysisData Load(string path)
		{
			string text = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<AnalysisData>(text);
		}

		public void SaveToWorldFolder(string worldFolder)
		{
			SaveToFile(GetDefaultPath(worldFolder));
		}

		public static AnalysisData LoadFromWorldFolder(string worldFolder)
		{
			return Load(GetDefaultPath(worldFolder));
		}

		public static bool ExistsInWorldFolder(string worldFolder)
		{
			return File.Exists(GetDefaultPath(worldFolder));
		}

		private static string GetDefaultPath(string worldFolder)
		{
			return Path.Combine(worldFolder, "analysis.data");
		}


	}
}

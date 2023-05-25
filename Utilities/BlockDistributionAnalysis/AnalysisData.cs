using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MCUtils.Utilities.BlockDistributionAnalysis
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

		public Dictionary<string, BlockCount> data = new Dictionary<string, BlockCount>();
		public int chunkCounter;

		private readonly object lockObj = new object();

		public void IncreaseCounter(string blockID, short y)
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

		public void IncreaseCounter(ProtoBlock block, short y)
		{
			IncreaseCounter(block.ID, y);
		}

		public long GetTotalAtY(string blockID, short y)
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

		public double GetRateAtY(string blockID, short y)
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
			File.WriteAllBytes(path, Save().ToArray());
		}

		private MemoryStream Save()
		{
			var stream = new MemoryStream();
			var bf = new BinaryFormatter();
			bf.Serialize(stream, this);
			return stream;
		}

		public static AnalysisData Load(string path)
		{
			using(var stream = new FileStream(path, FileMode.Open))
			{
				var bf = new BinaryFormatter();
				return (AnalysisData)bf.Deserialize(stream);
			}
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

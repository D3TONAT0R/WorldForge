using System.Collections.Generic;
using System.IO;
using System.Text;
using static MCUtils.Utilities.BlockDistributionAnalysis.AnalysisEvaluator;

namespace MCUtils.Utilities.BlockDistributionAnalysis
{
	public class AnalysisEvaluation
	{
		public class EvaluationEntry
		{
			public string name;
			public Dictionary<short, double> evaluationData;

			public EvaluationEntry(string name, Dictionary<short, double> data)
			{
				this.name = name;
				evaluationData = data;
			}
		}

		public readonly AnalysisData analysis;
		public short yMin;
		public short yMax;
		public List<EvaluationEntry> evaluations = new List<EvaluationEntry>();

		public AnalysisEvaluation(AnalysisData analysis, short yMin, short yMax)
		{
			this.analysis = analysis;
			this.yMin = yMin;
			this.yMax = yMax;
		}

		public void AddEvaluation(BlockGroup group)
		{
			var entry = new EvaluationEntry(group.name, Evaluate(analysis, group));
			evaluations.Add(entry);
		}

		public string GenerateResultsAsCSV()
		{
			char sep = ',';
			StringBuilder csv = new StringBuilder();
			csv.Append("");
			foreach(var entry in evaluations)
			{
				csv.Append(sep + entry.name);
			}
			csv.Append(sep + "Chunk count: " + analysis.chunkCounter);
			csv.AppendLine();

			for(short y = yMin; y < yMax; y++)
			{
				csv.Append(y);
				foreach(var ev in evaluations)
				{
					ev.evaluationData.TryGetValue(y, out double v);
					csv.Append(sep + v.ToString());
				}
				csv.AppendLine();
			}
			return csv.ToString();
		}

		public void SaveAsCSVInWorldFolder(string worldFolder)
		{
			string csv = GenerateResultsAsCSV();
			string path = Path.Combine(worldFolder, "block_distribution.csv");
			File.WriteAllText(path, csv);
		}
	}
}

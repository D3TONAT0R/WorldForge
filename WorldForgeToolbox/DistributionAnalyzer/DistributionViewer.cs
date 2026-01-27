using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldForge.IO;
using WorldForge.Regions;
using WorldForge.Utilities.BlockDistributionAnalysis;
using WorldForgeToolbox;

namespace RegionViewer.DistributionAnalyzer
{
	public partial class DistributionViewer : Form
	{
		public enum LineStyle
		{
			Normal,
			Dashed,
			Dotted,
			Special
		}

		static readonly Dictionary<string, (SKColor, LineStyle, bool)> chartColors = new Dictionary<string, (SKColor, LineStyle, bool)>()
		{
			{ "Coal", (new SKColor(0xFF767171), LineStyle.Normal, true) },
			{ "Iron", (new SKColor(0xFFD0C07A), LineStyle.Normal, true) },
			{ "Iron Block", (new SKColor(0xFFD0C07A), LineStyle.Special, true) },
			{ "Gold", (new SKColor(0xFFFFD966), LineStyle.Normal, true) },
			{ "Gold Block", (new SKColor(0xFFFFD966), LineStyle.Special, true) },
			{ "Copper", (new SKColor(0xFFB9A045), LineStyle.Normal, true) },
			{ "Copper Block", (new SKColor(0xFFB9A045), LineStyle.Special, true) },
			{ "Diamond", (new SKColor(0xFF69DBFF), LineStyle.Normal, true) },
			{ "Emerald", (new SKColor(0xFF02CA02), LineStyle.Normal, true) },
			{ "Lapis Lazuli", (new SKColor(0xFF0070C0), LineStyle.Normal, true) },
			{ "Redstone", (new SKColor(0xFFDA200C), LineStyle.Normal, true) },
			{ "Air", (new SKColor(0xFFBDD7EE), LineStyle.Dotted, false) },
			{ "Water", (new SKColor(0xFF0070C0), LineStyle.Dashed, false) },
			{ "Lava", (new SKColor(0xFFFC9804), LineStyle.Dashed, false) },
		};

		public DistributionViewer()
		{
			InitializeComponent();
			Test();
		}

		public void Test()
		{
			if (!OpenFileUtility.OpenRegionDialog(out var path)) return;
			var analyzer = new Analyzer();
			analyzer.AnalyzeRegion(RegionDeserializer.LoadMainRegion(path, null, loadChunks: true, chunkLoadFlags: ChunkLoadFlags.All));
			Show(analyzer.analysisData, AnalysisEvaluator.TargetBlockTypes.Ores, -64, 320, false);
		}

		public void Show(AnalysisData data, AnalysisEvaluator.TargetBlockTypes targetFlags, short yMin, short yMax, bool relativeToStone)
		{
			var evaluation = new AnalysisEvaluation(data, yMin, yMax, relativeToStone);
			foreach (var g in AnalysisEvaluator.GetTargetBlocks(targetFlags))
			{
				evaluation.AddEvaluation(g);
			}
			Show(evaluation);
		}

		public void Show(AnalysisEvaluation evaluation)
		{
			List<ISeries> series = new List<ISeries>();
			foreach(var eval in evaluation.evaluations)
			{
				var line = new LineSeries<ObservablePoint>()
				{
					Name = eval.name,
					Values = eval.evaluationData.Select(kvp => new ObservablePoint(kvp.Key, kvp.Value * 16 * 16)).OrderBy(pt => pt.X).ToArray(),
					LineSmoothness = 0,
					GeometryFill = null,
					GeometryStroke = null,
					GeometrySize = 0,
					Fill = null
				};
				if (chartColors.TryGetValue(eval.name, out var c)) {
					var stroke = new SolidColorPaint(c.Item1, 2);
					switch(c.Item2)
					{
						case LineStyle.Dashed:
							stroke.PathEffect = new DashEffect([10, 10]);
							break;
						case LineStyle.Dotted:
							stroke.PathEffect = new DashEffect([3, 5]);
							break;
						case LineStyle.Special:
							stroke.PathEffect = new DashEffect([2, 2]);
							break;
                    }
					line.Stroke = stroke;
				}
				series.Add(line);
			}
			chart.AnimationsSpeed = TimeSpan.FromMilliseconds(0);
			chart.Series = series;

		}
	}
}

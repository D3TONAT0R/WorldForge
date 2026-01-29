using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WinForms;
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
using WorldForge;
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
			{ "Amethyst", (new SKColor(0xFF8C68CA), LineStyle.Normal, true) },
			{ "Air", (new SKColor(0xFFBDD7EE), LineStyle.Dotted, false) },
			{ "Water", (new SKColor(0xFF0070C0), LineStyle.Dashed, false) },
			{ "Lava", (new SKColor(0xFFFC9804), LineStyle.Dashed, false) },
		};

		public DistributionViewer()
		{
			LiveCharts.Configure(cfg =>
			{
				cfg.AddDarkTheme();
				cfg.UseDefaults();
			});
			InitializeComponent();
			//Test();
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
			foreach (var eval in evaluation.evaluations)
			{
				if (eval.Maximum <= 0) continue;
				var line = new LineSeries<ObservablePoint>()
				{
					Name = eval.name,
					Values = ToPoints(eval),
					LineSmoothness = 0,
					GeometryFill = null,
					GeometryStroke = null,
					GeometrySize = 0,
					Fill = null,
					DataLabelsFormatter = DataLabelFormatter,
				};
				if (chartColors.TryGetValue(eval.name, out var c))
				{
					var stroke = new SolidColorPaint(c.Item1, 2);
					switch (c.Item2)
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
			chart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Right;
			chart.TooltipTextSize = 12;
			chart.Tooltip = new SKDefaultTooltip
			{
				Padding = new LiveChartsCore.Drawing.Padding(2),
				AnimationsSpeed = TimeSpan.FromMilliseconds(0)
			};
			chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
			chart.LegendTextSize = 12;
			var legend = new SKDefaultLegend
			{
				Padding = new LiveChartsCore.Drawing.Padding(2),
				AnimationsSpeed = TimeSpan.FromMilliseconds(0)
			};
			chart.Legend = legend;
			chart.Series = series;
			var xAxis = new Axis
			{
				Name = "Height",
				CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 3),
				CrosshairPadding = new LiveChartsCore.Drawing.Padding(4),
				CrosshairLabelsBackground = new LiveChartsCore.Drawing.LvcColor(255, 0, 0),
				CrosshairLabelsPaint = new SolidColorPaint(SKColors.White),
				CrosshairSnapEnabled = true,
				TextSize = 12,
				NameTextSize = 12,
				//CustomSeparators = [-64, -48, -32, -16, 0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 256, 272, 288, 304, 320],
				DrawTicksPath = true,
				TicksPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(64)),
				SubticksPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(32)),
			};
			bool logarithmic = true;
			Axis yAxis = logarithmic ? new LogarithmicAxis(10) : new Axis();
			yAxis.Name = "Percentage";
			yAxis.MinStep = 0.00001;
			yAxis.MinLimit = 0;
			yAxis.TextSize = 12;
			yAxis.NameTextSize = 12;
			yAxis.Labeler = d => d.ToString("P3");
			yAxis.ZeroPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(128), 3);

			chart.XAxes = [xAxis];
			chart.YAxes = [yAxis];
			chart.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both | LiveChartsCore.Measure.ZoomAndPanMode.NoFit;
			chart.AnimationsSpeed = TimeSpan.FromMilliseconds(250);
		}

		private static List<ObservablePoint> ToPoints(AnalysisEvaluation.EvaluationEntry eval)
		{
			var list = eval.evaluationData.Select(kvp => new ObservablePoint(kvp.Key, kvp.Value)).OrderBy(pt => pt.X).ToList();
			if (list.Count > 1)
			{
				int lowest = (int)list[0].X!;
				int highest = (int)list[^1].X!;
				//Fill in missing points with nulls
				for (int i = 0; i <= list.Count; i++)
				{
					int expectedY = lowest + i;
					if (expectedY > highest)
					{
						break;
					}
					if (i < list.Count)
					{
						int actualY = (int)list[i].X!;
						if (actualY > expectedY)
						{
							list.Insert(i, new ObservablePoint(expectedY, null));
						}
					}
					else
					{
						list.Add(new ObservablePoint(expectedY, null));
					}
				}
			}
			return list;
		}

		private string DataLabelFormatter(ChartPoint<ObservablePoint, CircleGeometry, LabelGeometry> arg)
		{
			double v = arg.Coordinate.PrimaryValue;
			return v.ToString("P3");
		}

		private void openButton_Click(object sender, EventArgs e)
		{
			using (var browser = new CreateAnalysisForm())
			{
				browser.ShowDialog();
				if (browser.FilePath == null) return;
				_ = RunAnalysis(browser);
			}
		}

		private async Task RunAnalysis(CreateAnalysisForm browser)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			var progressForm = ProgressWindow.Show(cancellationTokenSource);
			await Task.Run(() =>
			{
				var analyzer = new Analyzer();
				if (browser.WorldScanMode == CreateAnalysisForm.ScanMode.Region)
				{
					progressForm.UpdateText("Analyzing region...");
					var region = RegionDeserializer.LoadMainRegion(browser.FilePath, null, loadChunks: true, chunkLoadFlags: ChunkLoadFlags.Blocks);
					analyzer.AnalyzeRegion(region, true, cancellationTokenSource.Token);
				}
				else
				{
					var worldRoot = Path.GetDirectoryName(browser.FilePath);
					var dimension = Dimension.Load(null, worldRoot, browser.Dimension.SubdirectoryName, browser.Dimension);
					if (browser.WorldScanMode == CreateAnalysisForm.ScanMode.FullWorld)
					{
						int regionCount = dimension.regions.Count;
						progressForm.MaxProgressValue = regionCount;
						var progressReporter = new Progress<int>(value =>
						{
							progressForm.UpdateProgress(value, $"Analyzing dimension ... ({value}/{regionCount} regions)");
						});
						analyzer.AnalyzeDimension(dimension, true, cancellationTokenSource.Token, progressReporter);
					}
					else
					{
						int chunkCount = (browser.ScanChunkRadius * 2 + 1) * (browser.ScanChunkRadius * 2 + 1);
						progressForm.MaxProgressValue = chunkCount;
						var progressReporter = new Progress<int>(value =>
						{
							progressForm.UpdateProgress(value, $"Analyzing area ... ({value}/{chunkCount} chunks)");
						});
						analyzer.AnalyzeDimensionArea(dimension, browser.ScanOrigin.Chunk, browser.ScanChunkRadius, true, cancellationTokenSource.Token, progressReporter);
					}
				}
				progressForm.Close();
				Show(analyzer.analysisData, AnalysisEvaluator.TargetBlockTypes.Ores, -64, 320, false);
			}, cancellationTokenSource.Token);
		}

		private void zoomToFitButton_Click(object sender, EventArgs e)
		{
			var x = chart.XAxes.First();
			x.MinLimit = null;
			x.MaxLimit = null;
			var y = chart.YAxes.First();
			y.MinLimit = null;
			y.MaxLimit = null;
		}
	}
}

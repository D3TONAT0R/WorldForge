using WorldForge.Coordinates;
using WorldForge.Utilities.BlockDistributionAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldForge.ConsoleTools
{
	public class BlockDistributionConsoleTool : IConsoleTool
	{

		public void Run(string[] args)
		{
			string path;
			if(args.Length > 0)
			{
				path = args[0];
			}
			else
			{
				Console.WriteLine("Enter path to world folder:");
				path = Console.ReadLine().Replace("\"", "");
			}

			Console.WriteLine("Select a search pattern:");
			Console.WriteLine("[1] Ores only");
			Console.WriteLine("[2] Ores + Air + Liquids");
			Console.WriteLine("[3] Ores + Air + Liquids + Stones");
			Console.WriteLine("[4] Ores + Air + Liquids + Stones + Terrain");
			int choice;
			while(!GetMultipleChoice(out choice, ('1', 1), ('2', 2), ('3', 3), ('4', 4)))
			{
				Console.WriteLine("Invalid input.");
			}

			AnalysisEvaluator.TargetBlockTypes targetFlags = AnalysisEvaluator.TargetBlockTypes.Ores;
			if(choice >= 2) targetFlags |= AnalysisEvaluator.TargetBlockTypes.AirAndLiquids;
			if(choice >= 3) targetFlags |= AnalysisEvaluator.TargetBlockTypes.Stones;
			if(choice >= 4) targetFlags |= AnalysisEvaluator.TargetBlockTypes.TerrainBlocks;

			Console.WriteLine("Select chart type:");
			Console.WriteLine("[N] Normal (linear) chart");
			Console.WriteLine("[L] Logarithmic chart");
			Console.WriteLine("[B] Both");
			int logChoice;
			while(!GetMultipleChoice(out logChoice, ('N', 0), ('L', 1), ('B', 2)))
			{
				Console.WriteLine("Invalid input.");
			}

			Console.WriteLine("Select evaluation type:");
			Console.WriteLine("[N] Normal");
			Console.WriteLine("[R] Relative to stone (good for ores)");
			int evalChoice;
			while(!GetMultipleChoice(out evalChoice, ('N', 0), ('R', 1)))
			{
				Console.WriteLine("Invalid input.");
			}

			World.GetWorldInfo(path, out var worldName, out var version, out var regionLocations);
			Console.WriteLine($"World: {worldName}, Version: {version}");

			PerformAnalysis(targetFlags, path, regionLocations, -64, 320, logChoice, evalChoice == 1);
		}

		static void PerformAnalysis(AnalysisEvaluator.TargetBlockTypes targetFlags, string worldRoot, RegionLocation[] regions, short yMin, short yMax, int chartTypeFlags, bool relativeToStone)
		{
			Analyzer analyzer = new Analyzer(yMin, yMax);
			bool useExisting = false;
			if(AnalysisData.ExistsInWorldFolder(worldRoot))
			{
				Console.WriteLine($"Analysis data already exists for world '{worldRoot}'. Use existing analysis?");
				Console.WriteLine("[Y] Yes");
				Console.WriteLine("[N] No");
				if(Console.ReadKey(true).Key == ConsoleKey.Y)
				{
					useExisting = true;
				}
			}

			AnalysisData data;
			if(useExisting)
			{
				data = AnalysisData.LoadFromWorldFolder(worldRoot);
			}
			else
			{
				for(int ri = 0; ri < regions.Length; ri++)
				{
					Console.WriteLine($"Analyzing region {regions[ri].ToFileName()} [{ri + 1}/{regions.Length}]");
					try
					{
						var region = RegionDeserializer.LoadRegion(Path.Combine(worldRoot, "region", regions[ri].ToFileName()));
						analyzer.AnalyzeRegion(region);
					}
					catch(Exception e)
					{
						Console.WriteLine($"Failed to process region at {regions[ri].x}.{regions[ri].z}: {e.Message}");
					}
				}
				analyzer.analysisData.SaveToWorldFolder(worldRoot);
				data = analyzer.analysisData;
			}

			var evaluation = new AnalysisEvaluation(data, yMin, yMax, relativeToStone);
			foreach(var g in AnalysisEvaluator.GetTargetBlocks(targetFlags))
			{
				evaluation.AddEvaluation(g);
			}

			evaluation.SaveAsCSVInWorldFolder(worldRoot);

			string nameAdd = relativeToStone ? "rel" : null;

			//TODO: support charts
			/*
			if(chartTypeFlags == 0 || chartTypeFlags == 2)
			{
				ChartBuilder.SaveAsChartInWorldFolder(evaluation, worldRoot, nameAdd, false);
			}
			if(chartTypeFlags == 1 || chartTypeFlags == 2)
			{
				ChartBuilder.SaveAsChartInWorldFolder(evaluation, worldRoot, nameAdd + "(log)", true);
			}
			*/
		}

		static bool GetMultipleChoice<T>(out T result, params (char, T)[] validChoices)
		{
			var k = char.ToUpper(Console.ReadKey(true).KeyChar);
			foreach(var v in validChoices)
			{
				if(k == char.ToUpper(v.Item1))
				{
					result = v.Item2;
					return true;
				}
			}
			result = default;
			return false;
		}
	}
}

using MCUtils.Coordinates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCUtils.ConsoleTools
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
			while(!GetChoice(out choice))
			{
				Console.WriteLine("Invalid input.");
			}

			BlockDistributionAnalysis.TargetBlockTypes targetFlags = BlockDistributionAnalysis.TargetBlockTypes.Ores;
			if(choice >= 2) targetFlags |= BlockDistributionAnalysis.TargetBlockTypes.AirAndLiquids;
			if(choice >= 3) targetFlags |= BlockDistributionAnalysis.TargetBlockTypes.Stones;
			if(choice >= 4) targetFlags |= BlockDistributionAnalysis.TargetBlockTypes.TerrainBlocks;

			World.GetWorldInfo(path, out var worldName, out var version, out var regionLocations);
			Console.WriteLine($"World: {worldName}, Version: {version}");

			PerformAnalysis(targetFlags, path, regionLocations, -64, 320);
		}

		static void PerformAnalysis(BlockDistributionAnalysis.TargetBlockTypes targetFlags, string worldRoot, RegionLocation[] regions, short yMin, short yMax)
		{
			BlockDistributionAnalysis analysis = new BlockDistributionAnalysis(targetFlags, yMin, yMax);
			for(int ri = 0; ri < regions.Length; ri++)
			{
				Console.WriteLine($"Processing region {regions[ri].ToFileName()} [{ri + 1}/{regions.Length}]");
				try
				{
					var region = RegionLoader.LoadRegion(Path.Combine(worldRoot, "region", regions[ri].ToFileName()));
					Console.WriteLine("Region loaded, processing chunks ...");
					analysis.AnalyzeRegion(region);
				}
				catch(Exception e)
				{
					Console.WriteLine($"Failed to analyze region at {regions[ri].x}.{regions[ri].z}: {e.Message}");
				}
			}
			analysis.SaveAsCSVInWorldFolder(worldRoot);
			Console.WriteLine("Saved to " + worldRoot);
		}

		static bool GetChoice(out int choice)
		{
			var k = Console.ReadKey(true).KeyChar;
			if(k == '1')
			{
				choice = 1;
				return true;
			}
			else if(k == '2')
			{
				choice = 2;
				return true;
			}
			else if(k == '3')
			{
				choice = 3;
				return true;
			}
			else if(k == '4')
			{
				choice = 4;
				return true;
			}
			else
			{
				choice = 0;
				return false;
			}
		}
	}
}

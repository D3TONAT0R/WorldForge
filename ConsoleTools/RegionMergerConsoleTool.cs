using MCUtils.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MCUtils.ConsoleTools
{
	public class RegionMergerConsoleTool : IUtilTask
	{

		public void Run()
		{
			MCUtilsConsole.WriteLine("Enter path to region file 1:");
			string r1 = GetFilePath(false);
			if (r1 == null) return;
			MCUtilsConsole.WriteLine("Enter path to region file 2:");
			string r2 = GetFilePath(false);
			if (r2 == null) return;
			MCUtilsConsole.WriteLine("Enter path to map image:");
			string map = GetFilePath(false);
			if (map == null) return;
			MCUtilsConsole.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			MCUtilsConsole.WriteLine("Starting merge...");
			Region region1;
			Region region2;
			try
			{
				region1 = RegionLoader.LoadRegion(r1);
				region2 = RegionLoader.LoadRegion(r2);
			}
			catch (Exception e)
			{
				MCUtilsConsole.WriteError("Failed to open region file(s):");
				MCUtilsConsole.WriteError(e.ToString());
				return;
			}
			Bitmap mask = new Bitmap(map);
			var merger = new RegionMerger(region1, region2, mask);
			MCUtilsConsole.WriteLine("Merging ...");
			var mergedRegion = merger.Merge();
			MCUtilsConsole.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			RegionSerializer.WriteRegionToStream(mergedRegion, stream, Version.DefaultVersion);
			stream.Close();
			MCUtilsConsole.WriteLine("Done");
		}

		private string GetFilePath(bool isSaveLocation)
		{
			bool exit = false;
			while (!exit)
			{
				string file = MCUtilsConsole.GetInput().Replace("\"", "");
				if (file.StartsWith("exit")) break;
				if (!isSaveLocation)
				{
					if (File.Exists(file))
					{
						return file;
					}
					else
					{
						MCUtilsConsole.WriteWarning("Path is invalid. Try again.");
					}
				}
				else
				{
					if (Directory.Exists(Path.GetDirectoryName(file)))
					{
						return file;
					}
					else
					{
						MCUtilsConsole.WriteWarning("Directory does not exist. Try again.");
					}
				}
			}
			return null;
		}
	}
}

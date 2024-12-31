using ImageMagick;
using System;
using System.IO;
using WorldForge.IO;
using WorldForge.Regions;

namespace WorldForge.ConsoleTools
{
	public class RegionMergerConsoleTool : IConsoleTool
	{

		public void Run(string[] args)
		{
			WorldForgeConsole.WriteLine("Enter path to region file 1:");
			string r1 = GetFilePath(false);
			if(r1 == null) return;
			WorldForgeConsole.WriteLine("Enter path to region file 2:");
			string r2 = GetFilePath(false);
			if(r2 == null) return;
			WorldForgeConsole.WriteLine("Enter path to map image:");
			string map = GetFilePath(false);
			if(map == null) return;
			WorldForgeConsole.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			WorldForgeConsole.WriteLine("Starting merge...");
			Region region1;
			Region region2;
			try
			{
				region1 = RegionDeserializer.LoadRegion(r1, null);
				region2 = RegionDeserializer.LoadRegion(r2, null);
			}
			catch(Exception e)
			{
				WorldForgeConsole.WriteError("Failed to open region file(s):");
				WorldForgeConsole.WriteError(e.ToString());
				return;
			}
			MagickImage mask = new MagickImage(map);
			var merger = new RegionMerger(region1, region2, mask);
			WorldForgeConsole.WriteLine("Merging ...");
			var mergedRegion = merger.Merge();
			WorldForgeConsole.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			RegionSerializer.WriteRegionToStream(mergedRegion, stream, GameVersion.DefaultVersion);
			stream.Close();
			WorldForgeConsole.WriteLine("Done");
		}

		private string GetFilePath(bool isSaveLocation)
		{
			bool exit = false;
			while(!exit)
			{
				string file = WorldForgeConsole.GetInput().Replace("\"", "");
				if(file.StartsWith("exit")) break;
				if(!isSaveLocation)
				{
					if(File.Exists(file))
					{
						return file;
					}
					else
					{
						WorldForgeConsole.WriteWarning("Path is invalid. Try again.");
					}
				}
				else
				{
					if(Directory.Exists(Path.GetDirectoryName(file)))
					{
						return file;
					}
					else
					{
						WorldForgeConsole.WriteWarning("Directory does not exist. Try again.");
					}
				}
			}
			return null;
		}
	}
}

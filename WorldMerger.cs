using MCUtils.Coordinates;
using MCUtils.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static MCUtils.ChunkData;
using static MCUtils.World;

namespace MCUtils.ConsoleTools {
	public class WorldMerger : IConsoleTool {

		public class RegionRecord {
			public RegionLocation loc;

			public bool existsInWorld1;
			public bool existsInWorld2;

			public readonly string filename;

			public bool CanMerge => existsInWorld1 && existsInWorld2;

			public RegionRecord(WorldMerger merger, int rx, int rz) {
				loc = new RegionLocation(rx, rz);
				filename = $"r.{rx}.{rz}.mca";
				existsInWorld1 = File.Exists(Path.Combine(merger.world1Path, filename));
				existsInWorld2 = File.Exists(Path.Combine(merger.world2Path, filename));
			}
		}

		string world1Path;
		string world2Path;
		string outputPath;

		RegionLocation lowerBound;
		RegionLocation upperBound;

		public World mergedWorld;

		public void Run(string[] args) {
			MCUtilsConsole.WriteLine("Enter path to world 1's region files:");
			world1Path = GetFilePath(true);
			if(world1Path == null) return;
			MCUtilsConsole.WriteLine("Enter path to world 2's region files:");
			world2Path = GetFilePath(true);
			if(world2Path == null) return;

			MCUtilsConsole.WriteLine("Enter the lower coordinate for the merge (inclusive):");
			lowerBound = ParseCoordinates(MCUtilsConsole.GetInput());
			MCUtilsConsole.WriteLine("Enter the upper coordinate for the merge (inclusive):");
			upperBound = ParseCoordinates(MCUtilsConsole.GetInput());

			int sizeX = upperBound.x - lowerBound.x + 1;
			int sizeZ = upperBound.z - lowerBound.z + 1;

			MCUtilsConsole.WriteLine($"Enter path to map image (Must be {sizeX * 512}x{sizeZ * 512} or {sizeX * 32}x{sizeZ * 32}:");
			string map = GetFilePath(false);
			if(map == null) return;
			Bitmap worldMergeMap = new Bitmap(map);
			byte mergeMode = 0;
			if(worldMergeMap.Width == sizeX * 512 && worldMergeMap.Height == sizeZ * 512) mergeMode = 1;
			if(worldMergeMap.Width == sizeX * 32 && worldMergeMap.Height == sizeZ * 32) mergeMode = 2;
			if(mergeMode == 0) {
				MCUtilsConsole.WriteError("Map was not the correct size!");
				return;
			}

			MCUtilsConsole.WriteLine("Enter path to output file:");
			outputPath = GetFilePath(true);

			List<RegionRecord> records = new List<RegionRecord>();

			for(int rz = lowerBound.z; rz <= upperBound.z; rz++) {
				for(int rx = lowerBound.x; rx <= upperBound.x; rx++) {
					records.Add(new RegionRecord(this, rx, rz));
				}
			}

			int w1only = 0;
			int w2only = 0;
			int mergeable = 0;

			foreach(var r in records) {
				if(r.CanMerge) {
					mergeable++;
				} else {
					if(r.existsInWorld1) w1only++;
					else if(r.existsInWorld2) w2only++;
				}
			}

			if(w1only > 0) MCUtilsConsole.WriteWarning($"There are {w1only} regions that only exist in world 1.");
			if(w2only > 0) MCUtilsConsole.WriteWarning($"There are {w2only} regions that only exist in world 2.");
			MCUtilsConsole.WriteLine($"{mergeable} out of {records.Count} can be merged. Press any key to start.");
			Console.ReadKey();

			MCUtilsConsole.WriteLine("Starting world merge...");
			foreach(var r in records) {
				int localX = r.loc.x - lowerBound.x;
				int localZ = r.loc.z - lowerBound.z;
				int scale = mergeMode == 1 ? 512 : 32;
				Bitmap section = worldMergeMap.Clone(new Rectangle(localX*scale, localZ*scale, scale, scale), worldMergeMap.PixelFormat);

				MCUtilsConsole.WriteLine($"Merging {r.loc.x}.{r.loc.z}.mca ...");
				var region1 = RegionLoader.LoadRegion(Path.Combine(world1Path, r.filename));
				var region2 = RegionLoader.LoadRegion(Path.Combine(world2Path, r.filename));
				var merger = new RegionMerger(region1, region2, section);
				var mergedRegion = merger.Merge();

				MCUtilsConsole.WriteLine("Writing file...");
				FileStream stream = new FileStream(Path.Combine(outputPath, r.filename), FileMode.Create);
				RegionSerializer.WriteRegionToStream(mergedRegion, stream, Version.DefaultVersion);
				stream.Close();
			}
		}

		private RegionLocation ParseCoordinates(string input) {
			string[] split = input.Trim().Split(' ');
			if(split.Length < 2) throw new FormatException();
			return new RegionLocation(int.Parse(split[0]), int.Parse(split[1]));
		}

		private string GetFilePath(bool isDirectory) {
			bool exit = false;
			while(!exit) {
				string file = MCUtilsConsole.GetInput().Replace("\"", "");
				if(file.StartsWith("exit")) break;
				if(!isDirectory) {
					if(File.Exists(file)) {
						return file;
					} else {
						MCUtilsConsole.WriteWarning("Path is invalid. Try again.");
					}
				} else {
					if(Directory.Exists(Path.GetDirectoryName(file))) {
						return file;
					} else {
						MCUtilsConsole.WriteWarning("Directory does not exist. Try again.");
					}
				}
			}
			return null;
		}
	}
}
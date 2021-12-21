
using MCUtils.IO;
using System;
using System.IO;

namespace MCUtils {
	public class RandomBlockRegionGen : IUtilTask {

		Region region;

		public void Run() {
			MCUtilsConsole.WriteLine("Size of block patches (1-4):");
			size = int.Parse(MCUtilsConsole.GetInput());
			size = Math.Max(1, Math.Min(4, size));
			MCUtilsConsole.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			MCUtilsConsole.WriteLine("Starting...");
			region = new Region(0, 0);
			FillWithRandomBlocks();
			MCUtilsConsole.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			RegionSerializer.WriteRegionToStream(region, stream, Version.DefaultVersion);
			stream.Close();
			MCUtilsConsole.WriteLine("Done");
		}

		int size = 1;

		private void FillWithRandomBlocks() {
			Random random = new Random();
			for(int z = 0; z < 512; z += size) {
				for(int x = 0; x < 512; x += size) {
					for(int y = 0; y < size*4; y += size) {
						string block = BlockList.blocks[random.Next(0, BlockList.blocks.Length)];
						for(int i = 0; i < size; i++) {
							for(int j = 0; j < size; j++) {
								for(int k = 0; k < size; k++) {
									if(region.IsWithinBoundaries(x+i, y+j, z+k)) region.SetBlock(x+i, y+j, z+k, block);
								}
							}
						}
					}
				}
			}
		}

		private string GetFilePath(bool isSaveLocation) {
			bool exit = false;
			while(!exit) {
				string file = MCUtilsConsole.GetInput();
				if(file.StartsWith("exit")) break;
				if(!isSaveLocation) {
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
using System;
using System.IO;

namespace MCUtils {
	public class RandomBlockRegionGen : IUtilTask {

		Region region;

		public void Run() {
			Program.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			Program.WriteLine("Starting...");
			region = new Region();
			FillWithRandomBlocks();
			Program.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			region.WriteRegionToStream(stream, 0, 0);
			stream.Close();
			Program.WriteLine("Done");
		}

		private void FillWithRandomBlocks() {
			Random random = new Random();
			for(int z = 0; z < 512; z++) {
				for(int x = 0; x < 512; x++) {
					string block = BlockList.blocks[random.Next(0, BlockList.blocks.Length)];
					region.SetBlock(x, 0, z, block);
				}
			}
		}

		private string GetFilePath(bool isSaveLocation) {
			bool exit = false;
			while(!exit) {
				string file = Program.GetInput();
				if(file.StartsWith("exit")) break;
				if(!isSaveLocation) {
					if(File.Exists(file)) {
						return file;
					} else {
						Program.WriteWarning("Path is invalid. Try again.");
					}
				} else {
					if(Directory.Exists(Path.GetDirectoryName(file))) {
						return file;
					} else {
						Program.WriteWarning("Directory does not exist. Try again.");
					}
				}
			}
			return null;
		}
	}
}
using System;
using System.IO;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.Regions;

namespace WorldForge.ConsoleTools
{
	public class RandomBlockRegionGen : IConsoleTool
	{

		Region region;

		public void Run(string[] args)
		{
			WorldForgeConsole.WriteLine("Size of block patches (1-4):");
			size = int.Parse(WorldForgeConsole.GetInput());
			size = Math.Max(1, Math.Min(4, size));
			WorldForgeConsole.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			WorldForgeConsole.WriteLine("Starting...");
			region = new Region(0, 0);
			FillWithRandomBlocks();
			WorldForgeConsole.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			RegionSerializer.WriteRegionToStream(region, stream, GameVersion.DefaultVersion);
			stream.Close();
			WorldForgeConsole.WriteLine("Done");
		}

		int size = 1;

		private void FillWithRandomBlocks()
		{
			Random random = new Random();
			for(int z = 0; z < 512; z += size)
			{
				for(int x = 0; x < 512; x += size)
				{
					for(int y = 0; y < size * 4; y += size)
					{
						string block = BlockList.blocks[random.Next(0, BlockList.blocks.Length)];
						for(int i = 0; i < size; i++)
						{
							for(int j = 0; j < size; j++)
							{
								for(int k = 0; k < size; k++)
								{
									var pos = new BlockCoord(x + i, y + j, z + k);
									if(region.IsWithinBoundaries(pos))
									{
										region.SetBlock(pos, block);
									}
								}
							}
						}
					}
				}
			}
		}

		private string GetFilePath(bool isSaveLocation)
		{
			bool exit = false;
			while(!exit)
			{
				string file = WorldForgeConsole.GetInput();
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
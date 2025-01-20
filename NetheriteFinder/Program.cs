﻿using WorldForge;
using WorldForge.Coordinates;

namespace NetheriteFinder
{
	internal class Program
	{
		public class Vein
		{
			public BlockCoord pos;
			public int count;

			public Vein(BlockCoord pos, int count)
			{
				this.pos = pos;
				this.count = count;
			}

			public override string ToString()
			{
				return $"{pos} (Vein of {count})";
			}
		}

		static void Main(string[] args)
		{
			WorldForgeManager.Initialize();

			Console.WriteLine("Enter path to region cache");
			var cache = Console.ReadLine();

			while(true)
			{
				//Copy directory to temp folder
				var tempPath = Path.Combine(Path.GetTempPath(), "NetheriteFinder");
				if(Directory.Exists(tempPath))
				{
					Directory.Delete(tempPath, true);
				}
				Directory.CreateDirectory(tempPath);
				foreach(var file in Directory.GetFiles(cache))
				{
					File.Copy(file, Path.Combine(tempPath, Path.GetFileName(file)));
				}

				var dim = Dimension.FromRegionFolder(null, tempPath, DimensionID.Unknown, GameVersion.Release_1(21, 3));
				Console.WriteLine("Enter player position (x,z):");
				var pos = Console.ReadLine().Split(',');
				var px = int.Parse(pos[0].Trim());
				var pz = int.Parse(pos[1].Trim());
				Console.WriteLine("Search radius:");
				var radius = int.Parse(Console.ReadLine());
				List<BlockCoord> positions = new List<BlockCoord>();
				var block = BlockList.Find("ancient_debris");
				for(int x = px - radius; x <= px + radius; x++)
				{
					for(int z = pz - radius; z <= pz + radius; z++)
					{
						for(int y = 10; y < 18; y++)
						{
							var bc = new BlockCoord(x, y, z);
							if(dim.GetBlock(bc) == block)
							{
								positions.Add(bc);
							}
						}
					}
				}
				var player = new BlockCoord2D(px, pz);
				
				var veins = ToVeins(positions, 5);

				var sorted = veins.OrderBy(v => BlockCoord2D.Distance(player, v.pos.XZ)).ToArray();
				for(int i = 0; i < Math.Min(30, sorted.Length); i++)
				{
					var p = sorted.ElementAt(i);
					Console.WriteLine($"{block.ID.id} at {p}");
				}
				if(sorted.Length > 30)
				{
					Console.WriteLine($"{sorted.Length - 30} more ...");
				}

				Console.WriteLine("Press any key to repeat search");
				Console.ReadKey();
				Console.Clear();
			}
		}

		static List<Vein> ToVeins(List<BlockCoord> positions, int max)
		{
			List<Vein> veins = new List<Vein>();
			for(int i = 0; i < positions.Count; i++)
			{
				var pos = positions[i];
				float d;
				int c = ClosestVein(pos, veins, out d);
				if(c == -1 || d > max)
				{
					veins.Add(new Vein(pos, 1));
				}
				else
				{
					veins[c].count++;
				}
			}
			return veins;
		}

		static int ClosestVein(BlockCoord pos, List<Vein> veins, out float d)
		{
			d = float.MaxValue;
			int c = -1;
			for(int i = 0; i < veins.Count; i++)
			{
				float dist = BlockCoord.Distance(pos, veins[i].pos);
				if(dist < d)
				{
					d = dist;
					c = i;
				}
			}
			return c;
		}
	}
}
using WorldForge;
using WorldForge.Coordinates;

namespace NetheriteFinder
{
	internal class ConsoleApp
	{

		public static void Run(string[] args)
		{
			Console.WriteLine("Enter path to region cache");
			var regionDir = Console.ReadLine();

			List<int> reportedVeinHashes = new List<int>();
			BlockCoord lastVeinPos = BlockCoord.Zero;
			int lastRadius = 1;

			while(true)
			{
				
				Console.WriteLine("Enter player position (x z):");
				if(lastVeinPos != BlockCoord.Zero)
				{
					Console.WriteLine("or 'r' to repeat search from last reported ore vein");
				}
				var read = Console.ReadLine();
				int px;
				int pz;
				bool repeatLast = false;
				if(read.ToLower().StartsWith("r"))
				{
					repeatLast = true;
					px = lastVeinPos.x;
					pz = lastVeinPos.z;
				}
				else
				{
					var pos = read.Split(' ');
					px = int.Parse(pos[0].Trim());
					pz = int.Parse(pos[1].Trim());
				}
				int radius;
				if(!repeatLast)
				{
					Console.WriteLine("Search radius:");
					radius = int.Parse(Console.ReadLine());
				}
				else
				{
					radius = lastRadius;
				}
				lastRadius = radius;
				var report = Report.Create(regionDir, px, pz, radius, SearchProfile.Netherite);

				//Remove veins that were already reported
				var rm = report.veins.RemoveAll(v => reportedVeinHashes.Contains(v.GetHashCode()));
				foreach(var v in report.veins)
				{
					reportedVeinHashes.Add(v.GetHashCode());
				}
				if(rm > 0)
				{
					Console.WriteLine($"Removed {rm} already logged veins");
				}
				
				report.SortVeinsByDistance(px, pz);
				
				for(int i = 0; i < Math.Min(30, report.veins.Count); i++)
				{
					var p = report.veins[i];
					var fromPos = lastVeinPos != BlockCoord.Zero ? lastVeinPos : new BlockCoord(px, 0, pz);
					Report.GetBearing(fromPos, p.pos, out var angle, out var distance);
					Console.WriteLine($"Vein at {p} [{angle:000}°{distance:00}m]");
					lastVeinPos = p.pos;
				}
				if(report.veins.Count > 30)
				{
					Console.WriteLine($"{report.veins.Count - 30} more ...");
				}

				Console.WriteLine("Press any key to repeat search");
				Console.ReadKey();
				Console.Clear();
			}
		}
	}
}

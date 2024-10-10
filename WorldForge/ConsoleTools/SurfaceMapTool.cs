using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorldForge.Utilities;

namespace WorldForge.ConsoleTools
{
	public class SurfaceMapTool : IConsoleTool
	{
		public void Run(string[] args)
		{
			Console.WriteLine("Path to world folder:");
			string path = Console.ReadLine().Replace("\"", "");
			Console.WriteLine("Minimum x:");
			int xMin = int.Parse(Console.ReadLine());
			Console.WriteLine("Minimum z:");
			int zMin = int.Parse(Console.ReadLine());
			Console.WriteLine("Maximum x:");
			int xMax = int.Parse(Console.ReadLine());
			Console.WriteLine("Maximum z:");
			int zMax = int.Parse(Console.ReadLine());
			World w = World.Load(path);
			string mapPath = Path.Combine(path, "surface_map.png");
			SurfaceMapGenerator.GenerateSurfaceMap(w.Overworld, xMin, zMin, xMax - 1, zMax - 1, HeightmapType.SolidBlocks, true).Save(mapPath);
			Console.WriteLine("Map written to " + mapPath);
		}
	}
}

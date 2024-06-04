using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WorldForge
{
	public static class WorldForgeManager
	{
		public static bool Initialized { get; private set; }

		public static void Initialize(string resourcePath, IBitmapFactory bitmapFactory)
		{
			if(resourcePath == null) resourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");
			string blockListPath = Path.Combine(resourcePath, "blocks.csv");
			if(!File.Exists(blockListPath))
			{
				throw new FileNotFoundException("Could not find blocks.csv at " + blockListPath);
			}
			BlockList.Initialize(File.ReadAllText(blockListPath));

			if(bitmapFactory != null)
			{
				Bitmaps.BitmapFactory = bitmapFactory;
				string colorMapPath = Path.Combine(resourcePath, "colormap.png");
				if(!File.Exists(colorMapPath))
				{
					throw new FileNotFoundException("Could not find colormap.png at " + colorMapPath);
				}
				Blocks.InitializeColorMap(Path.Combine(resourcePath, "colormap.png"));
			}
			else
			{
				Console.WriteLine("WorldForge initialized without a bitmap factory. All bitmap related functions will not be available.");
			}

			Initialized = true;
		}
	}
}

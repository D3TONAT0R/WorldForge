using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WorldForge
{
	public static class WorldForgeManager
	{
		public static void Initialize(string resourcePath, IBitmapFactory bitmapFactory)
		{
			if(resourcePath == null) resourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");
			BlockList.Initialize(File.ReadAllText(Path.Combine(resourcePath, "blocks.csv")));
			Blocks.InitializeColorMap(Path.Combine(resourcePath, "colormap.tif"));
			Bitmaps.BitmapFactory = bitmapFactory;
		}
	}
}

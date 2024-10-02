using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using WorldForge.Items;

namespace WorldForge
{
	public static class WorldForgeManager
	{
		public static bool Initialized { get; private set; }

		public static void Initialize(IBitmapFactory bitmapFactory)
		{
			BlockList.Initialize(GetResourceAsText("blocks.csv"), GetResourceAsText("block_remappings.csv"));
			ItemList.Initialize(GetResourceAsText("items.csv"));

			if(bitmapFactory != null)
			{
				Bitmaps.BitmapFactory = bitmapFactory;
				Blocks.InitializeColorMap(GetResource("colormap.png"));
			}
			else
			{
				Console.WriteLine("WorldForge initialized without a bitmap factory. All bitmap related functions will not be available.");
			}
			Initialized = true;
		}

		private static Stream GetResource(string fileName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("WorldForge.Resources." + fileName);
		}

		private static string GetResourceAsText(string fileName)
		{
			using(Stream stream = GetResource(fileName))
			{
				using(StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}

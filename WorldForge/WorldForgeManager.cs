using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorldForge.Biomes;
using WorldForge.Items;

namespace WorldForge
{
	public static class WorldForgeManager
	{
		public static bool Initialized { get; private set; }

		public static int MaxDegreeOfParallelism
		{
			get => ParallelOptions.MaxDegreeOfParallelism;
			set => ParallelOptions.MaxDegreeOfParallelism = value;
		}

		public static ParallelOptions ParallelOptions { get; private set; } = new ParallelOptions
		{
			MaxDegreeOfParallelism = Environment.ProcessorCount
		};

		public static void Initialize()
		{
			BlockList.Initialize(GetResourceAsText("blocks.csv"), GetResourceAsText("block_remappings.csv"));
			ItemList.Initialize(GetResourceAsText("items.csv"));
			BiomeIDs.Initialize(GetResourceAsText("biomes.csv"));
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

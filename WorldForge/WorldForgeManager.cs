using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WorldForge.Biomes;
using WorldForge.Items;
using WorldForge.Maps;

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

		public static IResourceLoader CustomResourceLoader { get; set; } = null;

		public static void Initialize(IResourceLoader resourceLoader = null)
		{
			CustomResourceLoader = resourceLoader;
			BlockList.Initialize(GetResourceAsText("blocks.csv"), GetResourceAsText("block_remappings.csv"));
			ItemList.Initialize(GetResourceAsText("items.csv"));
			BiomeIDs.Initialize(GetResourceAsText("biomes.csv"));
			MapColorPalette.InitializePalettes();

#if !IMAGESHARP_DISABLED
			Logger.Verbose("Using ImageSharp for Bitmap processing");
			Bitmaps.BitmapFactory = new ImageSharpBitmapFactory();
#endif
			Initialized = true;
		}

		public static Stream GetResource(string fileName)
		{
			if(CustomResourceLoader != null)
			{
				var stream = CustomResourceLoader.GetResourceAsStream(fileName);
				if(stream != null) return stream;
				else Logger.Verbose($"Custom resource loader did not provide a stream for '{fileName}'. Falling back to embedded resources.");
			}
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("WorldForge.Resources." + fileName);
		}

		public static string GetResourceAsText(string fileName)
		{
			if(CustomResourceLoader != null)
			{
				var text = CustomResourceLoader.GetResourceAsText(fileName);
				if(text != null) return text;
				else Logger.Verbose($"Custom resource loader did not provide text for '{fileName}'. Falling back to embedded resources.");
			}
			using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WorldForge.Resources." + fileName))
			{
				using(StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}

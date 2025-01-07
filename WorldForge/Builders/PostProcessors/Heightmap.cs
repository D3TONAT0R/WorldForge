
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorldForge;

namespace WorldForge.Builders.PostProcessors
{
	public class Heightmap
	{
		public readonly byte[,] heights;

		public Heightmap(int sizeX, int sizeZ)
		{
			heights = new byte[sizeX, sizeZ];
		}

		public static Heightmap FromImage(string path, int offsetX = 0, int offsetZ = 0, int sizeX = -1, int sizeZ = -1, ColorChannel channel = ColorChannel.Red)
		{
			var image = Image.Load<Rgba32>(path);
			var map = new Heightmap(sizeX, sizeZ);
			for(int x = 0; x < sizeX; x++)
			{
				for(int z = 0; z < sizeZ; z++)
				{
					map.heights[x, z] = image[x, z].GetChannel(channel);
				}
			}
			return map;
		}
	}
}
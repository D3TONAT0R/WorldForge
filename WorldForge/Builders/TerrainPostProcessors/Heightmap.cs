namespace WorldForge.Builders.PostProcessors
{
	public class Heightmap
	{
		public readonly byte[,] heights;

		public Heightmap(int sizeX, int sizeZ)
		{
			heights = new byte[sizeX, sizeZ];
		}

		public static Heightmap FromImage(string path, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var byteBuffer = BitmapUtils.GetBitmapBytes(path, out int w, out int h, out int d);
			var map = new Heightmap(sizeX, sizeZ);
			for(int x = 0; x < sizeX; x++)
			{
				for(int z = 0; z < sizeZ; z++)
				{
					map.heights[x, z] = BitmapUtils.GetPixel(byteBuffer, x + offsetX, z + offsetZ, w, d).R;
				}
			}
			return map;
		}
	}
}
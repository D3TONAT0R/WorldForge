using System;

namespace WorldForge
{
	public static class Extensions
	{

		public static int ChunkCoord(this int i)
		{
			return (int)Math.Floor(i / 16d);
		}

		public static int RegionCoord(this int i)
		{
			return (int)Math.Floor(i / 512d);
		}
	}
}

using System;

namespace MCUtils
{
	public static class Extensions {

		public static int ChunkCoord(this int i) {
			return (int)Math.Floor(i / 16f);
		}

		public static int RegionCoord(this int i) {
			return (int)Math.Floor(i / 512f);
		}

		public static int Mod(this int i, int m)
		{
			int r = i % m;
			return r < 0 ? r + m : r;
		}

		public static short Mod(this short i, short m)
		{
			short r = (short)(i % m);
			return r < 0 ? (short)(r + m) : r;
		}
	}
}

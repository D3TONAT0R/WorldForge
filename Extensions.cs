using System;

namespace MCUtils {
	public static class Extensions {

		public static string Last(this string[] arr) {
			return arr[arr.Length - 1];
		}

		public static int ChunkCoord(this int i) {
			return (int)Math.Floor(i / 16f);
		}

		public static int RegionCoord(this int i) {
			return (int)Math.Floor(i / 512f);
		}
	}
}

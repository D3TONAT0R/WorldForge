using System;
using WorldForge.Coordinates;

namespace WorldForge
{
	public static class SeededRandom
	{
		//TODO: produces visible repeating patterns on the XZ axis
		// Linear Congruential Generator (LCG) parameters
		private const long A = 6364136223846793005;
		private const long C = 1;
		private const long M = 1L << 32;

		public static double Double(long seed, int x, int y = 0, int z = 0)
		{
			return Value(GetSeed(seed, x, y, z));
		}

		public static double Double(long seed, BlockCoord pos) => Double(seed, pos.x, pos.y, pos.z);

		public static double Double(double max, long seed, int x, int y = 0, int z = 0)
		{
			return Double(seed, x, y, z) * max;
		}

		public static double Double(double max, long seed, BlockCoord pos) => Double(max, seed, pos.x, pos.y, pos.z);

		public static int Int(int maxExclusive, long seed, int x, int y = 0, int z = 0)
		{
			var d = Double(seed, x, y, z);
			return (int)(d * maxExclusive);
		}

		public static int Int(int maxExclusive, long seed, BlockCoord pos) => Int(maxExclusive, seed, pos.x, pos.y, pos.z);

		public static long Long(long maxExclusive, long seed, int x, int y = 0, int z = 0)
		{
			var d = Double(seed, x, y, z);
			return (long)(d * maxExclusive);
		}

		public static long Long(long maxExclusive, long seed, BlockCoord pos) => Long(maxExclusive, seed, pos.x, pos.y, pos.z);

		public static double Range(double min, double max, long seed, int x, int y = 0, int z = 0)
		{
			return min + Double(max - min, seed, x, y, z);
		}

		public static double Range(double minInclusive, double maxExclusive, long seed, BlockCoord pos) => Range(minInclusive, maxExclusive, seed, pos.x, pos.y, pos.z);

		public static int RangeInt(int minInclusive, int maxExclusive, long seed, int x, int y = 0, int z = 0)
		{
			return minInclusive + Int(maxExclusive - minInclusive, seed, x, y, z);
		}

		public static int RangeInt(int minInclusive, int maxExclusive, long seed, BlockCoord pos) => RangeInt(minInclusive, maxExclusive, seed, pos.x, pos.y, pos.z);

		public static bool Probability(float prob, long seed, int x, int y = 0, int z = 0)
		{
			return Double(seed, x, y, z) <= prob;
		}

		public static bool Probability(float prob, long seed, BlockCoord pos) => Probability(prob, seed, pos.x, pos.y, pos.z);

		public static bool ProbabilityInt(int i, long seed, int x, int y = 0, int z = 0)
		{
			return Int(i, seed, x, y, z) == 0;
		}

		public static bool ProbabilityInt(int i, long seed, BlockCoord pos) => ProbabilityInt(i, seed, pos.x, pos.y, pos.z);

		private static double Value(long seed)
		{
			// Update the seed using the LCG formula
			seed = (seed * A + C) % M;

			// Normalize the seed to the range [0.0, 1.0]
			return Math.Abs((double)seed / M);
		}

		private static long GetSeed(long seed, int x, int y, int z)
		{
			// Combine the x, y, and seed values into a single seed value
			unchecked
			{
				return x * 341873128712 + y * 132897987541 + z * 7121522197 + seed * 9214253;
			}
		}
	}
}
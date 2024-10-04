using System;
using System.Collections.Generic;

namespace WorldForge.Biomes
{
	public static class BiomeIDs
	{
		private static List<BiomeID> biomeRegistry;

		public static void Initialize(string biomeData)
		{
			biomeRegistry = new List<BiomeID>();
			//TODO: load biomes from file
		}

		public BiomeID Get(string id)
		{

		}

		public static BiomeID ParseBiome(string biomeID)
		{
			if (TryParseBiome(biomeID, out var biome))
			{
				return biome;
			}
			else
			{
				throw new ArgumentException($"Unrecognized biome ID: '{biomeID}'");
			}
		}

		public static bool TryParseBiome(string biomeID, out BiomeID biome)
		{
			switch (biomeID)
			{
				default:
					return Enum.TryParse(biomeID, true, out biome);
			}
		}
	}
}

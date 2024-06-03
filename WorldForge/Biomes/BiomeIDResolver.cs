using System;

namespace WorldForge.Biomes
{
	public static class BiomeIDResolver
	{
		public static string GetIDForVersion(BiomeID biome, GameVersion gameVersion)
		{
			switch (biome)
			{
				case BiomeID.mountains:
					if (gameVersion < GameVersion.Release_1(13)) return "extreme_hills";
					else if (gameVersion < GameVersion.Release_1(18)) return "mountains";
					else return "windswept_hills";
				default:
					return biome.ToString();
			}
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

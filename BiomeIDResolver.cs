using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils
{
	public static class BiomeIDResolver
	{
		public static string GetIDForVersion(BiomeID biome, Version gameVersion)
		{
			switch(biome)
			{
				case BiomeID.mountains:
					if(gameVersion < Version.Release_1(13)) return "extreme_hills";
					else if(gameVersion < Version.Release_1(18)) return "mountains";
					else return "windswept_hills";
				default:
					return biome.ToString();
			}
		}

		public static BiomeID ParseBiome(string biomeID)
		{
			if(TryParseBiome(biomeID, out var biome))
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
			switch(biomeID)
			{
				default:
					return Enum.TryParse(biomeID, true, out biome);
			}
		}
	}
}

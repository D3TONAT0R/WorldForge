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
			throw new NotImplementedException();
		}

		public static BiomeID Get(string id, bool throwError = false)
		{
			foreach(var biome in biomeRegistry)
			{
				if(biome.CheckID(id))
				{
					return biome;
				}
			}
			if(throwError) throw new ArgumentException($"Unrecognized biome ID: '{id}'");
			return null;
		}

		public static BiomeID GetFromNumeric(byte id, bool throwError = false)
		{
			foreach(var biome in biomeRegistry)
			{
				if(biome.numericId == id)
				{
					return biome;
				}
			}
			if(throwError) throw new ArgumentException($"Unrecognized biome numeric ID: '{id}'");
			return null;
		}

		public static bool TryGet(string id, out BiomeID biome)
		{
			biome = Get(id, false);
			return biome != null;
		}

		public static BiomeID GetOrCreate(string id)
		{
			foreach(var biome in biomeRegistry)
			{
				if(biome.CheckID(id))
				{
					return biome;
				}
			}
			var newBiome = new BiomeID(id);
			biomeRegistry.Add(newBiome);
			return newBiome;
		}
	}
}

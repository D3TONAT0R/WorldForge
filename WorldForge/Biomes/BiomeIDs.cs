using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WorldForge.Biomes
{
	public static class BiomeIDs
	{
		private static ConcurrentDictionary<int, BiomeID> biomeRegistry;

		public static void Initialize(string biomeData)
		{
			Logger.Verbose("Initializing biome list ...");
			biomeRegistry = new ConcurrentDictionary<int, BiomeID>();
			var csv = new CSV(biomeData);
			foreach(var row in csv.data)
			{
				//ID;Numeric ID;Pre-flattening ID;Pre-1.18 ID;Added in Version;Fallback
				string id = row[0];
				row.TryGetByte(1, out var numeric);
				row.TryGetString(2, out var preFlatteningId);
				row.TryGetString(3, out var pre118Id);
				GameVersion? addedVersion = null;
				if(row.TryGetString(4, out var versionString)) addedVersion = GameVersion.Parse(versionString);
				BiomeID substitute = null;
				if(row.TryGetString(5, out var substituteID))
				{
					substitute = Get(substituteID, true);
				}
				if(TryGet(id, out _))
				{
					throw new ArgumentException($"Duplicate biome ID: '{id}'");
				}
				var biome = new BiomeID(id, numeric, preFlatteningId, pre118Id, substitute, addedVersion);
				Register(biome);
			}
		}

		public static BiomeID Get(string id, bool throwError = true)
		{
			foreach(var biome in biomeRegistry.Values)
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
			foreach(var biome in biomeRegistry.Values)
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
			foreach(var biome in biomeRegistry.Values)
			{
				if(biome.CheckID(id))
				{
					return biome;
				}
			}
			var newBiome = new BiomeID(id);
			if(newBiome.id.StartsWith("minecraft:"))
			{
				Logger.Warning($"Unrecognized biome '{id}', adding to list.");
			}
			Register(newBiome);
			return newBiome;
		}

		private static void Register(BiomeID b)
		{
			biomeRegistry.AddOrUpdate(b.GetHashCode(), b, UpdateValue);
		}

		private static BiomeID UpdateValue(int hash, BiomeID b)
		{
			return b;
		}
	}
}

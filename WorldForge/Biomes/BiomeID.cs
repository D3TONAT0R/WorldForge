namespace WorldForge.Biomes
{
	public class BiomeID
	{
		public static BiomeID Ocean => BiomeIDs.Get("ocean");
		public static BiomeID Plains => BiomeIDs.Get("plains");
		public static BiomeID NetherWastes => BiomeIDs.Get("nether_wastes");
		public static BiomeID TheEnd => BiomeIDs.Get("the_end");
		public static BiomeID TheVoid => BiomeIDs.Get("the_void");

		public readonly string id;
		public readonly byte? numericId;
		public readonly string preFlatteningId;
		public readonly string pre118Id;
		public readonly GameVersion addedInVersion;
		public BiomeID substitute;

		internal BiomeID(string id, byte? numericId = null)
		{
			this.id = id;
			this.numericId = numericId;
		}

		internal BiomeID(string id, byte? numericId, string preFlatteningId, string pre118Id, BiomeID substitute, GameVersion addedInVersion)
		{
			this.id = id;
			this.numericId = numericId;
			this.preFlatteningId = preFlatteningId;
			this.pre118Id = pre118Id;
			this.addedInVersion = addedInVersion;
			this.substitute = substitute;
		}

		public string GetIDForVersion(GameVersion version)
		{
			if(version < GameVersion.FirstFlatteningVersion && !string.IsNullOrEmpty(preFlatteningId))
			{
				return preFlatteningId;
			}
			else if(version < GameVersion.Release_1(18) && !string.IsNullOrEmpty(pre118Id))
			{
				return pre118Id;
			}
			else return id;
		}

		public string ResolveIDForVersion(GameVersion version)
		{
			BiomeID b = this;
			Resolve(version, ref b);
			if(b == null) b = Plains;
			return b.GetIDForVersion(version);
		}

		public static void Resolve(GameVersion version, ref BiomeID biome)
		{
			if(version < biome.addedInVersion)
			{
				if(biome.substitute != null)
				{
					biome = biome.substitute;
					Resolve(version, ref biome);
				}
				else
				{
					biome = null;
				}
			}
		}

		public bool CheckID(string id)
		{
			if(this.id == id)
			{
				return true;
			}
			else if(preFlatteningId != null && preFlatteningId == id)
			{
				return true;
			}
			else if(pre118Id != null && pre118Id == id)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override string ToString()
		{
			return id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
}

using System;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSourceData
	{
		public enum SourceRegionType
		{
			AlphaChunk,
			MCRegion,
			AnvilRegion
		}

		public SourceRegionType sourceRegionType;
		public readonly NBTFile main;
		public readonly NBTFile entities;
		public readonly NBTFile poi;

		public ChunkSourceData(NBTFile main, NBTFile entities, NBTFile poi, SourceRegionType sourceRegionType)
		{
			if (main == null) throw new NullReferenceException("Main NBT data must not be null.");
			this.main = main;
			this.entities = entities;
			this.poi = poi;
			this.sourceRegionType = sourceRegionType;
		}
	}
}
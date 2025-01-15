using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_14 : ChunkSerializer_1_13
	{
		public override bool SeparatePOIData => true;

		public ChunkSerializer_1_14(GameVersion version) : base(version) { }

		public override void LoadPointsOfInterest(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			//TODO: load POI data
		}

		public override void WritePointsOfInterest(ChunkData c, NBTCompound chunkNBT)
		{
			//TODO: write POI data
		}
	}
}

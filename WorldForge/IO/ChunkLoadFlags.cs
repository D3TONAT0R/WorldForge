using System;

namespace WorldForge.IO
{
	[Flags]
	public enum ChunkLoadFlags
	{
		None = 0,
		Blocks = 1 << 0,
		Entities = 1 << 1,
		TileEntities = 1 << 2,
		Biomes = 1 << 3,
		POI = 1 << 4,
		All = Blocks | Entities | TileEntities | Biomes | POI
	}
}
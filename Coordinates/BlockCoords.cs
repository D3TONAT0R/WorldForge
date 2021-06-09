using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct BlockCoords
	{
		public int x;
		public int y;
		public int z;

		public RegionCoords Region => new RegionCoords((int)Math.Floor(x / 512f), (int)Math.Floor(z / 512f));

		public ChunkCoords Chunk => new ChunkCoords((int)Math.Floor(x / 16f), (int)Math.Floor(z / 16f));

		public BlockCoords LocalRegionCoords => new BlockCoords(x % 512, y, z % 512);

		public BlockCoords LocalChunkCoords => new BlockCoords(x % 16, y, z % 16);

		public BlockCoords(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}
}

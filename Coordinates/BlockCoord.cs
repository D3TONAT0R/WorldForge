using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct BlockCoord
	{
		public int x;
		public int y;
		public int z;

		public RegionCoord Region => new RegionCoord((int)Math.Floor(x / 512f), (int)Math.Floor(z / 512f));

		public ChunkCoord Chunk => new ChunkCoord((int)Math.Floor(x / 16f), (int)Math.Floor(z / 16f));

		public BlockCoord LocalRegionCoords => new BlockCoord(x % 512, y, z % 512);

		public BlockCoord LocalChunkCoords => new BlockCoord(x % 16, y, z % 16);

		public BlockCoord(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}
}

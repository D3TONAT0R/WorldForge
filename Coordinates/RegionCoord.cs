using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct RegionCoord
	{
		public int x;
		public int z;

		public RegionCoord(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public ChunkCoord GetChunkCoord(int chunkOffsetX = 0, int chunkOffsetZ = 0)
		{
			return new ChunkCoord(x * 32 + chunkOffsetX, z * 32 + chunkOffsetZ);
		}
	}
}

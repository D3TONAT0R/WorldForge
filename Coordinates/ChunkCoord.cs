using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct ChunkCoord
	{
		public int x;
		public int z;

		public BlockCoord BlockCoord => new BlockCoord(x * 16, 0, z * 16);

		public ChunkCoord(int x, int z)
		{
			this.x = x;
			this.z = z;
		}
	}
}

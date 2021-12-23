using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct ChunkCoord
	{
		public int x;
		public int z;

		public ChunkCoord(int x, int z)
		{
			this.x = x;
			this.z = z;
		}
	}
}

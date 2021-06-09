using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Coordinates
{
	public struct ChunkCoords
	{
		public int x;
		public int z;

		public ChunkCoords(int x, int z)
		{
			this.x = x;
			this.z = z;
		}
	}
}

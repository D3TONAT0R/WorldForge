namespace WorldForge.Coordinates
{
	public struct ChunkCoord
	{
		public int x;
		public int z;

		public BlockCoord BlockCoord => new BlockCoord(x * 16, 0, z * 16);

		public ChunkCoord LocalRegionPos => new ChunkCoord(x.Mod(32), z.Mod(32));

		public ChunkCoord(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public override string ToString()
		{
			return $"{x},{z}";
		}
	}
}

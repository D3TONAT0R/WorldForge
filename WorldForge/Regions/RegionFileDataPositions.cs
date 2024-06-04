namespace WorldForge.Regions
{
	public class RegionFileDataPositions
	{
		public struct ChuknkDataPos
		{
			public uint offset;
			public byte sectorCount;

			public bool Exists => offset > 0 && sectorCount > 0;
		}

		public readonly string sourceFilePath;
		public readonly ChuknkDataPos[,] positions = new ChuknkDataPos[32, 32];

		public RegionFileDataPositions(string sourceFilePath)
		{
			this.sourceFilePath = sourceFilePath;
		}
	}
}

namespace WorldForge
{
    public struct Boundary
    {
        public int xMin;
        public int zMin;
        public int xMax;
        public int zMax;

        public int LengthX => xMax - xMin;
		public int LengthZ => zMax - zMin;

        public int CenterX => (xMin + xMax) / 2;
		public int CenterZ => (zMin + zMax) / 2;

		public Boundary(int xMin, int zMin, int xMax, int zMax)
        {
            this.xMin = xMin;
            this.zMin = zMin;
            this.xMax = xMax;
            this.zMax = zMax;
        }
    }
}
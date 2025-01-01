namespace WorldForge
{
    public struct Boundary
    {
        public int xMin;
        public int zMin;
        public int xMax;
        public int zMax;

        public Boundary(int xMin, int zMin, int xMax, int zMax)
        {
            this.xMin = xMin;
            this.zMin = zMin;
            this.xMax = xMax;
            this.zMax = zMax;
        }
    }
}
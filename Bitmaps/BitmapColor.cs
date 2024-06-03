namespace WorldForge
{
	public struct BitmapColor
	{
		public byte r, g, b, a;

		public BitmapColor(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public BitmapColor(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			a = 255;
		}
	}
}

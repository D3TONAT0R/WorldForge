using System;

namespace WorldForge
{
	public struct BitmapColor
	{
		public byte r, g, b, a;

		public byte this[ColorChannel c]
		{
			get
			{
				switch(c)
				{
					case ColorChannel.Red: return r;
					case ColorChannel.Green: return g;
					case ColorChannel.Blue: return b;
					case ColorChannel.Alpha: return a;
					default: throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch(c)
				{
					case ColorChannel.Red: r = value; break;
					case ColorChannel.Green: g = value; break;
					case ColorChannel.Blue: b = value; break;
					case ColorChannel.Alpha: a = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

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

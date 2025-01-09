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

		public byte this[int i]
		{
			get => this[(ColorChannel)i];
			set => this[(ColorChannel)i] = value;
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

		public static int GetClosestMapping(BitmapColor c, BitmapColor[] colorPalette)
		{
			int[] deviations = new int[colorPalette.Length];
			for(int i = 0; i < colorPalette.Length; i++)
			{
				deviations[i] += Math.Abs(c.r - colorPalette[i].r);
				deviations[i] += Math.Abs(c.g - colorPalette[i].g);
				deviations[i] += Math.Abs(c.b - colorPalette[i].b);
			}
			int index = 0;
			int closest = 999;
			for(byte i = 0; i < colorPalette.Length; i++)
			{
				if(deviations[i] < closest)
				{
					index = i;
					closest = deviations[i];
				}
			}
			return index;
		}

		public static int GetDitheredMapping(int x, int y, BitmapColor c, BitmapColor[] colorPalette, int ditherLimit)
		{
			float[] probs = new float[colorPalette.Length];
			for(int i = 0; i < colorPalette.Length; i++)
			{
				int deviation = 0;
				deviation += Math.Abs(c.r - colorPalette[i].r);
				deviation += Math.Abs(c.g - colorPalette[i].g);
				deviation += Math.Abs(c.b - colorPalette[i].b);
				if(deviation >= ditherLimit)
				{
					probs[i] = 0;
				}
				else
				{
					probs[i] = 1 - (deviation / (float)ditherLimit);
				}
			}
			float max = 0;
			foreach(float p in probs) max += p;
			double d = SeededRandom.Double(x, y, 0) * max;
			double v = 0;
			for(byte i = 0; i < probs.Length; i++)
			{
				v += probs[i];
				if(d < v) return i;
			}
			return 255;
		}
	}
}

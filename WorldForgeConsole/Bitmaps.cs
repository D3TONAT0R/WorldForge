using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldForge;
using Color = System.Drawing.Color;

namespace WorldForgeConsole
{
	public class WFBitmapFactory : IBitmapFactory
	{
		public IBitmap Create(int width, int height)
		{
			return new WFBitmap(width, height);
		}

		public IBitmap Load(string path)
		{
			return new WFBitmap(Bitmap.FromFile(path) as Bitmap);
		}

		public IBitmap LoadFromStream(Stream stream)
		{
			return new WFBitmap(Bitmap.FromStream(stream) as Bitmap);
		}
	}

	public class WFBitmap : IBitmap
	{
		private Bitmap bmp;

		public int Width => bmp.Width;
		public int Height => bmp.Height;

		public WFBitmap(int width, int height)
		{
			bmp = new Bitmap(width, height);
		}

		public WFBitmap(Bitmap bmp)
		{
			this.bmp = bmp;
		}

		public void SetPixel(int x, int y, BitmapColor c)
		{
			bmp.SetPixel(x, y, Color.FromArgb(c.a, c.r, c.g, c.b));
		}

		public BitmapColor GetPixel(int x, int y)
		{
			var col = bmp.GetPixel(x, y);
			return new BitmapColor(col.R, col.G, col.B, col.A);
		}

		public IBitmap Clone()
		{
			return new WFBitmap(new Bitmap(bmp));
		}

		public IBitmap CloneArea(int x, int y, int width, int height)
		{
			return new WFBitmap(bmp.Clone(new Rectangle(x, y, width, height), bmp.PixelFormat));
		}

		public void Save(string path)
		{
			bmp.Save(path);
		}

		public void Save(Stream stream)
		{
			bmp.Save(stream, ImageFormat.Png);
		}
	}
}

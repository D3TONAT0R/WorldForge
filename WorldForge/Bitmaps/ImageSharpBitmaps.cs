#if !IMAGESHARP_DISABLED
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorldForge;

namespace WorldForgeConsole
{
	public class ImageSharpBitmapFactory : IBitmapFactory
	{
		public IBitmap Create(int width, int height)
		{
			return new ImageSharpBitmap(width, height);
		}

		public IBitmap Load(string path)
		{
			return new ImageSharpBitmap(Image.Load<Rgba32>(path));
		}

		public IBitmap LoadFromStream(Stream stream)
		{
			return new ImageSharpBitmap(Image.Load<Rgba32>(stream));
		}
	}

	public class ImageSharpBitmap : IBitmap
	{
		private Image<Rgba32> bmp;

		public int Width => bmp.Width;
		public int Height => bmp.Height;

		public ImageSharpBitmap(int width, int height)
		{
			bmp = new Image<Rgba32>(width, height);
		}

		public ImageSharpBitmap(Image<Rgba32> bmp)
		{
			this.bmp = bmp;
		}

		public void SetPixel(int x, int y, BitmapColor c)
		{
			bmp[x, y] = new Rgba32(c.r, c.g, c.b, c.a);
		}

		public BitmapColor GetPixel(int x, int y)
		{
			var col = bmp[x, y];
			return new BitmapColor(col.R, col.G, col.B, col.A);
		}

		public IBitmap Clone()
		{
			return new ImageSharpBitmap(bmp);
		}

		public IBitmap CloneArea(int x, int y, int width, int height)
		{
			return new ImageSharpBitmap(bmp.Clone(i => i.Crop(new Rectangle(x, y, width, height))));
		}

		public void Save(string path)
		{
			bmp.Save(path);
		}

		public void Save(Stream stream)
		{
			bmp.SaveAsPng(stream);
		}
	}
}
#endif
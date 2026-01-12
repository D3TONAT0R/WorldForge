using System.Drawing.Imaging;
using WorldForge;

namespace WorldForgeToolbox
{
	public class WinformsBitmapFactory : IBitmapFactory
	{
		public IBitmap Create(int width, int height) => new WinformsBitmap(width, height);

		public IBitmap Load(string path) => new WinformsBitmap(new Bitmap(Image.FromFile(path)));

		public IBitmap LoadFromStream(Stream stream) => new WinformsBitmap(new Bitmap(Image.FromStream(stream)));
}

	public class WinformsBitmap : IBitmap
	{
		public readonly Bitmap bitmap;

		public int Width => bitmap.Width;

		public int Height => bitmap.Height;

		public WinformsBitmap(Bitmap bitmap)
		{
			this.bitmap = bitmap;
		}

		public WinformsBitmap(int width, int height)
		{
			bitmap = new Bitmap(width, height);
		}

		public void SetPixel(int x, int y, BitmapColor color) => bitmap.SetPixel(x, y, FromBitmapColor(color));

		public BitmapColor GetPixel(int x, int y) => ToBitmapColor(bitmap.GetPixel(x, y));

		public IBitmap Clone() => new WinformsBitmap((Bitmap)bitmap.Clone());

		public IBitmap CloneArea(int x, int y, int width, int height) => new WinformsBitmap((Bitmap)bitmap.Clone());

		public void Save(string path) => bitmap.Save(path, ImageFormat.Png);

		public void Save(Stream stream) => bitmap.Save(stream, ImageFormat.Png);

		private static BitmapColor ToBitmapColor(Color color) => new BitmapColor(color.R, color.G, color.B, color.A);

		private static Color FromBitmapColor(BitmapColor color) => Color.FromArgb(color.a, color.r, color.g, color.b);
	}
}
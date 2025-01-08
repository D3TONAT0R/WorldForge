namespace WorldForge
{
	public interface IBitmap
	{
		int Width { get; }

		int Height { get; }

		void SetPixel(int x, int y, BitmapColor c);

		BitmapColor GetPixel(int x, int y);

		IBitmap Clone();

		IBitmap CloneArea(int x, int y, int width, int height);

		void Save(string path);

		void Save(System.IO.Stream stream);
	}
}

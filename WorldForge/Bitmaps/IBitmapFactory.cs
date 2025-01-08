namespace WorldForge
{
	public interface IBitmapFactory
	{
		IBitmap Create(int width, int height);

		IBitmap Load(string path);

		IBitmap LoadFromStream(System.IO.Stream stream);
	}
}
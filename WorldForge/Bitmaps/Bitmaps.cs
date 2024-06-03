using System;
using System.IO;

namespace WorldForge
{
	public static class Bitmaps
	{
		public static IBitmapFactory BitmapFactory { get; set; }

		public static IBitmap Create(int width, int height)
		{
			if(BitmapFactory == null)
			{
				throw new InvalidOperationException("BitmapFactory not set");
			}
			return BitmapFactory.Create(width, height);
		}

		public static IBitmap Load(string path)
		{
			if(BitmapFactory == null)
			{
				throw new InvalidOperationException("BitmapFactory not set");
			}
			return BitmapFactory.Load(path);
		}

		public static IBitmap LoadFromStream(Stream stream)
		{
			if(BitmapFactory == null)
			{
				throw new InvalidOperationException("BitmapFactory not set");
			}
			return BitmapFactory.LoadFromStream(stream);
		}
	}
}

using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace WorldForge
{
	public static class BitmapUtils
	{
		public static byte[] GetBitmapBytes(string bitmapPath, out int width, out int height, out int depth)
		{
			using(FileStream stream = File.Open(bitmapPath, FileMode.Open))
			{
				using(var bmp = new Bitmap(stream))
				{
					Bitmap splat = new Bitmap(stream);
					return GetBitmapBytes(splat, out width, out height, out depth);
				}
			}
		}

		public static byte[] GetBitmapBytes(Bitmap bmp, out int width, out int height, out int depth)
		{
			byte[] byteBuffer;
			width = bmp.Width;
			height = bmp.Height;
			depth = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
			var rect = new Rectangle(0, 0, width, height);
			var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
			byteBuffer = new byte[height * width * depth];
			Marshal.Copy(data.Scan0, byteBuffer, 0, byteBuffer.Length);
			return byteBuffer;
		}

		public static Color GetPixel(byte[] byteBuffer, int x, int y, int width, int depth)
		{
			int pos = (y * width + x) * depth;

			var b = byteBuffer[pos + 0];
			var g = byteBuffer[pos + 1];
			var r = byteBuffer[pos + 2];
			var a = depth > 3 ? byteBuffer[pos + 3] : (byte)255;

			return Color.FromArgb(a, r, g, b);
		}
	}
}

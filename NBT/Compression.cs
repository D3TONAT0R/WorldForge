using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCUtils.NBT
{
	public static class Compression
	{
		/// <summary>
		/// Creates a decompression stream for the given Zlib compressed bytes, commonly used for chunk data in region files.
		/// </summary>
		public static Stream CreateZlibDecompressionStream(byte[] bytes)
		{
			return new MemoryStream(ZlibStream.UncompressBuffer(bytes));
		}

		/// <summary>
		/// Creates a decompression stream for the given Zlib compressed bytes, commonly used for NBT data (*.dat) files.
		/// </summary>
		public static Stream CreateGZipDecompressionStream(byte[] bytes)
		{
			return new MemoryStream(GZipStream.UncompressBuffer(bytes));
		}

		/// <summary>
		/// Decompresses the given Zlib compressed byte array, commonly used for chunk data in region files.
		/// </summary>
		public static byte[] DecompressZlibBytes(byte[] compressed)
		{
			return ZlibStream.UncompressBuffer(compressed);
		}

		/// <summary>
		/// Decompresses the given GZip compressed byte array, commonly used for NBT data (*.dat) files.
		/// </summary>
		public static byte[] DecompressGZipBytes(byte[] compressed)
		{
			return GZipStream.UncompressBuffer(compressed);
		}

		/// <summary>
		/// Compresses the given uncompressed byte array with Zlib compression, commonly used for chunk data in region files.
		/// </summary>
		public static byte[] CompressZlibBytes(byte[] uncompressed)
		{
			return ZlibStream.CompressBuffer(uncompressed);
		}

		/// <summary>
		/// Compresses the given uncompressed byte array with GZip compression, commonly used for NBT data (*.dat) files.
		/// </summary>
		public static byte[] CompressGZipBytes(byte[] uncompressed)
		{
			return GZipStream.CompressBuffer(uncompressed);
		}
	}
}

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using System.IO.Compression;

namespace WorldForge.NBT
{
	public static class Compression
	{
		/// <summary>
		/// Creates a decompression stream for the given Zlib compressed bytes, commonly used for chunk data in region files.
		/// </summary>
		public static Stream CreateZlibDecompressionStream(Stream compressed)
		{
			return CreateMemoryStream(new InflaterInputStream(compressed));
		}

		/// <summary>
		/// Creates a decompression stream for the given Zlib compressed bytes, commonly used for NBT data (*.dat) files.
		/// </summary>
		public static Stream CreateGZipDecompressionStream(Stream compressed)
		{
			return CreateMemoryStream(new GZipInputStream(compressed));
		}

		/// <summary>
		/// Compresses the given uncompressed byte array with Zlib compression, commonly used for chunk data in region files.
		/// </summary>
		public static Stream CreateZlibCompressionStream(Stream uncompressed)
		{
			uncompressed.Position = 0;
			var ms = new MemoryStream();
			using(var deflaterStream = new DeflaterOutputStream(ms) { IsStreamOwner = false })
			{
				uncompressed.CopyTo(deflaterStream);
			}
			if(ms.Length < 5) throw new InvalidDataException("Compressed data is too small.");
			return ms;
		}

		/// <summary>
		/// Compresses the given uncompressed byte array with GZip compression, commonly used for NBT data (*.dat) files.
		/// </summary>
		public static Stream CreateGZipCompressionStream(Stream uncompressed)
		{
			uncompressed.Position = 0;
			var ms = new MemoryStream();
			using(var gzip = new GZipOutputStream(ms) { IsStreamOwner = false })
			{
				uncompressed.CopyTo(gzip);
			}
			if(ms.Length < 5) throw new InvalidDataException("Compressed data is too small.");
			return ms;
		}

		private static MemoryStream CreateMemoryStream(Stream stream)
		{
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			ms.Position = 0;
			return ms;
		}
	}
}

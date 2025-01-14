using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public static class RegionSerializer
	{

		/// <summary>
		/// Serializes a region in region file format to the given stream.
		/// </summary>
		/// <param name="region">The region to serialize.</param>
		/// <param name="main">The stream to which the serialized data should be written to.</param>
		/// <param name="version">The target game version.</param>
		/// <param name="progressReportCallback">If set, regularly reports back the number of chunks that were serialized. Maximum progress is 1024.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void WriteRegionToStream(Region region, FileStream main, FileStream entities, FileStream poi, GameVersion version, Action<int> progressReportCallback = null)
		{
			//TODO: incorporate entities and poi files
			Stopwatch stopwatch = Stopwatch.StartNew();

			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 8192; i++)
			{
				main.WriteByte(0);
			}

			var chunkSerializer = ChunkSerializer.GetForVersion(version);

			MemoryStream[] serializedChunks = new MemoryStream[1024];
			for(int z = 0; z < 32; z++)
			{
				Parallel.For(0, 32, WorldForgeManager.ParallelOptions, x =>
				{
					int i = z * 32 + x;
					var chunk = region.chunks[x, z];
					if(chunk != null)
					{
						var memoryStream = new MemoryStream(4096);
						serializedChunks[i] = memoryStream;

						var chunkData = chunkSerializer.CreateChunkNBT(chunk);
						var dv = version.GetDataVersion();
						if(dv.HasValue) chunkData.contents.Add("DataVersion", dv.Value);

						byte[] compressed = NBTSerializer.SerializeAsZlib(chunkData, false);
						var cLength = BitUtils.ToBigEndian(BitConverter.GetBytes(compressed.Length));
						memoryStream.Write(cLength, 0, cLength.Length);
						memoryStream.WriteByte(2);
						memoryStream.Write(compressed, 0, compressed.Length);
					}
				});
				progressReportCallback?.Invoke(z * 32);
			}
			progressReportCallback?.Invoke(1024);

			main.Position = 8192;
			for(int i = 0; i < 1024; i++)
			{
				if (serializedChunks[i] == null) continue;
				var serialized = serializedChunks[i];

				locations[i] = (int)(main.Position / 4096);
				byte size = (byte)Math.Ceiling(serialized.Length / 4096d);
				if(size == 0) throw new InvalidOperationException("Blank serialized chunk data detected.");
				sizes[i] = size;

				serialized.Position = 0;
				serialized.CopyTo(main);
				var padding = serialized.Length % 4096;
				//Pad the data to the next 4096 byte mark
				if(padding > 0)
				{
					byte[] paddingBytes = new byte[4096 - padding];
					main.Write(paddingBytes, 0, paddingBytes.Length);
				}
			}

			//Last modified timestamp
			//TODO: remember and keep previous timestamp if nothing was changed in the chunk
			DateTime currentTime = DateTime.UtcNow;
			int unixTime32 = (int)((DateTimeOffset)currentTime).ToUnixTimeSeconds();
			byte[] timestampBytes = BitUtils.ToBigEndian(BitConverter.GetBytes(unixTime32));

			main.Position = 0;

			for(int i = 0; i < 1024; i++)
			{
				//3 byte offset, 1 byte size
				byte[] offsetBytes = BitUtils.ToBigEndian(BitConverter.GetBytes(locations[i]));
				main.Write(offsetBytes, 1, 3);
				main.WriteByte(sizes[i]);
			}

			main.Position = 4096;
			for(int i = 0; i < 1024; i++)
			{
				main.Write(timestampBytes, 0, 4);
			}

			stopwatch.Stop();
			var duration = stopwatch.Elapsed.TotalSeconds;
			WorldForgeConsole.WriteLine($"Generating Region file took {duration:F2} seconds.");
		}
	}
}

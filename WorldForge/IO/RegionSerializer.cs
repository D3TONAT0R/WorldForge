using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WorldForge.Chunks;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public static class RegionSerializer
	{

		public static void WriteRegionFiles(Region region, GameVersion version, string mainPath, string entitiesPath, string poiPath)
		{
			using(var streams = new RegionFileStreams(mainPath, entitiesPath, poiPath))
			{
				WriteRegionToStreams(region, version, streams);
			}
		}

		/// <summary>
		/// Serializes a region in region file format to the given stream.
		/// </summary>
		/// <param name="region">The region to serialize.</param>
		/// <param name="main">The stream to which the serialized data should be written to.</param>
		/// <param name="version">The target game version.</param>
		/// <param name="progressReportCallback">If set, regularly reports back the number of chunks that were serialized. Maximum progress is 1024.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void WriteRegionToStreams(Region region, GameVersion version, RegionFileStreams streams)
		{
			//TODO: incorporate entities and poi files
			Stopwatch stopwatch = Stopwatch.StartNew();

			var chunkSerializer = ChunkSerializer.GetForVersion(version);
			var serializedMainChunks = new MemoryStream[1024];
			var serializedEntitiesChunks = chunkSerializer.SeparateEntitiesData && HasEntities(region) ? new MemoryStream[1024] : null;
			var serializedPOIChunks = chunkSerializer.SeparatePOIData && HasPOIs(region) ? new MemoryStream[1024] : null;

			Parallel.For(0, 32, WorldForgeManager.ParallelOptions, z =>
			{
				for(int x = 0; x < 32; x++)
				{
					int i = z * 32 + x;
					var chunk = region.chunks[x, z];
					if(chunk != null)
					{
						chunkSerializer.CreateChunkNBTs(chunk, out var mainFile, out var entitiesFile, out var poiFile);
						serializedMainChunks[i] = ToZlibStream(mainFile);
						if(entitiesFile != null && serializedEntitiesChunks != null)
						{
							serializedEntitiesChunks[i] = ToZlibStream(entitiesFile);
						}
						if(poiFile != null && serializedPOIChunks != null)
						{
							serializedPOIChunks[i] = ToZlibStream(poiFile);
						}
					}
				}
			});

			WriteRegionFile(streams.main, serializedMainChunks);

			stopwatch.Stop();
			var duration = stopwatch.Elapsed.TotalSeconds;
			Logger.Info($"Generating Region file took {duration:F2} seconds.");
		}

		private static bool HasEntities(Region r)
		{
			for(int z = 0; z < 32; z++)
			{
				for(int x = 0; x < 32; x++)
				{
					if(r.chunks[x, z]?.Entities?.Count > 0) return true;
				}
			}
			return false;
		}

		private static bool HasPOIs(Region r)
		{
			for(int z = 0; z < 32; z++)
			{
				for(int x = 0; x < 32; x++)
				{
					if(r.chunks[x, z]?.POIs?.Count > 0) return true;
				}
			}
			return false;
		}

		private static MemoryStream ToZlibStream(NBTFile file)
		{
			var memoryStream = new MemoryStream(4096);
			byte[] compressed = NBTSerializer.SerializeAsZlib(file, false);
			var cLength = BitUtils.ToBigEndian(BitConverter.GetBytes(compressed.Length));
			memoryStream.Write(cLength, 0, cLength.Length);
			memoryStream.WriteByte(2);
			memoryStream.Write(compressed, 0, compressed.Length);
			return memoryStream;
		} 

		private static void WriteRegionFile(Stream stream, MemoryStream[] serializedChunks)
		{
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 8192; i++)
			{
				stream.WriteByte(0);
			}

			stream.Position = 8192;
			for(int i = 0; i < 1024; i++)
			{
				if(serializedChunks[i] == null) continue;
				var serialized = serializedChunks[i];

				locations[i] = (int)(stream.Position / 4096);
				byte size = (byte)Math.Ceiling(serialized.Length / 4096d);
				if(size == 0) throw new InvalidOperationException("Blank serialized chunk data detected.");
				sizes[i] = size;

				serialized.Position = 0;
				serialized.CopyTo(stream);
				var padding = serialized.Length % 4096;
				//Pad the data to the next 4096 byte mark
				if(padding > 0)
				{
					byte[] paddingBytes = new byte[4096 - padding];
					stream.Write(paddingBytes, 0, paddingBytes.Length);
				}
			}

			//Last modified timestamp
			//TODO: remember and keep previous timestamp if nothing was changed in the chunk
			DateTime currentTime = DateTime.UtcNow;
			int unixTime32 = (int)((DateTimeOffset)currentTime).ToUnixTimeSeconds();
			byte[] timestampBytes = BitUtils.ToBigEndian(BitConverter.GetBytes(unixTime32));

			stream.Position = 0;

			for(int i = 0; i < 1024; i++)
			{
				//3 byte offset, 1 byte size
				byte[] offsetBytes = BitUtils.ToBigEndian(BitConverter.GetBytes(locations[i]));
				stream.Write(offsetBytes, 1, 3);
				stream.WriteByte(sizes[i]);
			}

			stream.Position = 4096;
			for(int i = 0; i < 1024; i++)
			{
				stream.Write(timestampBytes, 0, 4);
			}
		}
	}
}

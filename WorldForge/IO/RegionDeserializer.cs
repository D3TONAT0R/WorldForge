using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public static class RegionDeserializer
	{
		public class RegionData
		{
			public class CompressedChunkData
			{
				public uint length;
				public byte compressionType;
				public byte[] compressedChunk;
			}

			public RegionLocation position;
			public readonly uint[] chunkLocations = new uint[1024];
			public readonly byte[] chunkSizes = new byte[1024];
			public readonly long expectedEOF;

			public readonly CompressedChunkData[] compressedChunks = new CompressedChunkData[1024];

			public RegionData(Stream stream, string filepath)
			{
				RegionLocation.TryGetFromFileName(filepath, out position);

				Logger.Verbose($"Getting chunk locations and sizes for '{filepath}' ...");
				if(stream.Length < 4096)
				{
					//If the file is smaller than 4096 bytes, it is not a valid region file.
					Logger.Error($"Region file {filepath} is not a valid region file. Expected at least 4096 bytes, got {stream.Length}.");
					return;
				}
				for(uint i = 0; i < 1024; i++)
				{
					chunkLocations[i] = Read3ByteInt(stream);
					chunkSizes[i] = ReadNext(stream);
				}

				//Ignore timestamps between 4096 and 8192

				Logger.Verbose("Getting compressed chunk data ...");
				expectedEOF = 8192;
				for(int i = 0; i < 1024; i++)
				{
					if(chunkLocations[i] > 0)
					{
						stream.Seek(chunkLocations[i] * 4096, SeekOrigin.Begin);
						try
						{
							compressedChunks[i] = new CompressedChunkData()
							{
								length = ReadInt(stream),
								compressionType = ReadNext(stream),
								compressedChunk = ReadNext(stream, chunkSizes[i] * 4096 - 5)
							};
						}
						catch
						{
							//A partially generated region may contain a chunk whose stream position is out of range.
							//Can it be safely ignored? NBTExplorer doesn't read that chunk either and it often
							//seems to be located in non-generated areas of the region.
							Logger.Verbose("Ignoring out of range chunk at index " + i);

							//throw new FileLoadException($"Failed to load chunk at [{i % 32},{i / 32}].");
						}
						expectedEOF = (chunkLocations[i] + chunkSizes[i]) * 4096;
					}
				}
				Logger.Verbose("Finished reading region data from " + filepath);
			}

			public NBTFile GetChunkNBT(int i)
			{
				if(compressedChunks[i] == null) return null;
				using(var chunkStream = Compression.CreateZlibDecompressionStream(compressedChunks[i].compressedChunk))
				{
					return new NBTFile(chunkStream);
				}
			}
			
			public bool HasChunkAt(int i)
			{
				return compressedChunks[i] != null;
			}
		}

		private static SemaphoreSlim ioSemaphore = new SemaphoreSlim(1, 1);

		public static Region PreloadRegion(RegionFilePaths filePaths, Dimension parent, GameVersion? worldSaveVersion = null)
		{
			Logger.Verbose($"Preloading region from {filePaths.mainPath} ...");
			if(!RegionLocation.TryGetFromFileName(filePaths.mainPath, out var loc))
			{
				Logger.Error("Unable to interpret region location from file name: " + filePaths.mainPath);
			}
			Region region = Region.CreateExisting(loc, parent, filePaths);
			region.versionHint = worldSaveVersion;
			return region;
		}

		public static Region LoadRegion(RegionFilePaths filePaths, Dimension parent,
			GameVersion? worldSaveVersion = null, bool loadChunks = false, bool loadOrphanChunks = false,
			ChunkLoadFlags chunkLoadFlags = ChunkLoadFlags.All)
		{
			var r = PreloadRegion(filePaths, parent, worldSaveVersion);
			LoadRegionContent(r, loadChunks, loadOrphanChunks, chunkLoadFlags);
			return r;
		}

		public static Region LoadMainRegion(string file, Dimension parent, GameVersion? worldSaveVersion = null, bool loadChunks = false, bool loadOrphanChunks = false, ChunkLoadFlags chunkLoadFlags = ChunkLoadFlags.All)
		{
			var paths = new RegionFilePaths(file, null, null);
			return LoadRegion(paths, parent, worldSaveVersion, loadChunks, loadOrphanChunks, chunkLoadFlags);
		}

		public static void LoadRegionContent(Region region, bool loadChunks = false, bool loadOrphanChunks = false, ChunkLoadFlags loadFlags = ChunkLoadFlags.All)
		{
			Logger.Verbose($"Loading region content ...");
			RegionData main, entities, poi;
			using (var streams = region.sourceFilePaths.OpenStreams(ioSemaphore))
			{
				main = new RegionData(streams.main, region.sourceFilePaths.mainPath);
				entities = streams.entities != null ? new RegionData(streams.entities, region.sourceFilePaths.entitiesPath) : null;
				poi = streams.poi != null ? new RegionData(streams.poi, region.sourceFilePaths.poiPath) : null;
			}
			region.InitializeChunks();
			Logger.Verbose("Loading chunks ...");
			var extension = Path.GetExtension(region.sourceFilePaths.mainPath).ToLower();
			bool isAnvilFormat;
			if(extension == ".mcr")
			{
				isAnvilFormat = false;
			}
			else if(extension == ".mca")
			{
				isAnvilFormat = true;
			}
			else
			{
				Logger.Error($"Unknown region file format for file {region.sourceFilePaths.mainPath}. Expected .mcr or .mca extension.");
				isAnvilFormat = false;
			}
			Parallel.For(0, 1024, WorldForgeManager.ParallelOptions, i =>
			{
				LoadRegionChunk(region, loadChunks, i, main, entities, poi, loadFlags, isAnvilFormat);
			});
		}

		private static void LoadRegionChunk(Region region, bool loadChunks, int i, RegionData main, RegionData entities, RegionData poi, ChunkLoadFlags loadFlags, bool isAnvilFormat)
		{
			if(main.compressedChunks[i] != null)
			{
				Stopwatch sw = Logger.Level == LogLevel.Verbose ? Stopwatch.StartNew() : null;
				var format = isAnvilFormat ? ChunkSourceData.SourceRegionType.AnvilRegion : ChunkSourceData.SourceRegionType.MCRegion;
				var sources = new ChunkSourceData(main.GetChunkNBT(i), entities?.GetChunkNBT(i), poi?.GetChunkNBT(i), format);
				var coord = new ChunkCoord(i % 32, i / 32);
				region.chunks[coord.x, coord.z] = Chunk.CreateFromNBT(region, coord, sources, region.versionHint, loadChunks, loadFlags);
				if(sw != null)
				{
					sw.Stop();
					Logger.Verbose($"Loading chunk [{coord.x},{coord.z}] took {sw.ElapsedMilliseconds} ms.");
				}
			}
		}

		public static Region LoadRegionAlphaChunks(string worldSaveDir, RegionLocation location)
		{
			Logger.Verbose($"Loading region at {location} from Alpha world save directory {worldSaveDir} ...");
			List<(ChunkCoord, string)> chunkFileLocations = new List<(ChunkCoord, string)>();
			var lowerChunkCoord = location.GetChunkCoord();
			for(int z = lowerChunkCoord.z; z < lowerChunkCoord.z + 32; z++)
			{
				for(int x = lowerChunkCoord.x; x < lowerChunkCoord.x + 32; x++)
				{
					//TODO: likely won't work with chunks beyond +/- 127
					string xf = BitUtils.EncodeBase36(GetPositive2sComplement((sbyte)x));
					string zf = BitUtils.EncodeBase36(GetPositive2sComplement((sbyte)z));
					string file = $"c.{BitUtils.EncodeBase36(x)}.{BitUtils.EncodeBase36(z)}.dat";
					string path = Path.Combine(worldSaveDir, xf, zf, file);
					if(File.Exists(path))
					{
						chunkFileLocations.Add((new ChunkCoord(x, z), path));
					}
				}
			}

			Logger.Verbose($"Found {chunkFileLocations.Count} chunk files in region {location}.");
			var reg = Region.CreateNew(location, null);
			var cs = new ChunkSerializerAlpha(GameVersion.Alpha_1(0));
			Parallel.ForEach(chunkFileLocations, c =>
			{
				var sw = Logger.Level == LogLevel.Verbose ? Stopwatch.StartNew() : null;
				var coord = c.Item1;
				//TODO: not sure if path is correct
				var path = c.Item2;
				var regionSpacePos = new ChunkCoord(coord.x & 31, coord.z & 31);
				var chunk = Chunk.CreateFromNBT(reg, regionSpacePos, new ChunkSourceData(new NBTFile(path), null, null, ChunkSourceData.SourceRegionType.AlphaChunk));
				reg.chunks[regionSpacePos.x, regionSpacePos.z] = chunk;
				if(sw != null)
				{
					sw.Stop();
					Logger.Verbose($"Loading chunk [{coord.x},{coord.z}] took {sw.ElapsedMilliseconds} ms.");
				}
			});
			return reg;
		}

		///<summary>Loads chunk data from a specified index in a region file (for debugging purposes)</summary>
		public static NBTFile LoadChunkDataAtIndex(string filepath, int index)
		{
			RegionData rd;
			using(var stream = File.Open(filepath, FileMode.Open))
			{
				rd = new RegionData(stream, filepath);
			}
			using(var chunkStream = Compression.CreateZlibDecompressionStream(rd.compressedChunks[index].compressedChunk))
			{
				return new NBTFile(chunkStream);
			}
		}

		private static uint Read3ByteInt(Stream stream)
		{
			List<byte> bytes = new List<byte>(ReadNext(stream, 3));
			bytes.Insert(0, 0);
			return BitConverter.ToUInt32(BitUtils.ToBigEndian(bytes.ToArray()), 0);
		}

		private static uint ReadInt(Stream stream)
		{
			return (uint)BitConverter.ToInt32(BitUtils.ToBigEndian(ReadNext(stream, 4)), 0);

		}

		private static byte ReadNext(Stream stream)
		{
			int r = stream.ReadByte();
			if(r >= 0)
			{
				return (byte)r;
			}
			else
			{
				throw new EndOfStreamException();
			}
		}

		private static byte[] ReadNext(Stream stream, int count)
		{
			byte[] b = new byte[count];
			for(int i = 0; i < count; i++)
			{
				b[i] = ReadNext(stream);
			}
			return b;
		}

		private static byte GetPositive2sComplement(sbyte b) => unchecked((byte)b);
	}
}

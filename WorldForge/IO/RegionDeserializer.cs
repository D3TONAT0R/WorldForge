using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public static class RegionDeserializer
	{
		private class RegionData
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

				if(stream.Length < 4096) return;
				for(uint i = 0; i < 1024; i++)
				{
					chunkLocations[i] = Read3ByteInt(stream);
					chunkSizes[i] = ReadNext(stream);
				}

				//Ignore timestamps between 4096 and 8192

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

							//throw new FileLoadException($"Failed to load chunk at [{i % 32},{i / 32}].");
						}
						expectedEOF = (chunkLocations[i] + chunkSizes[i]) * 4096;
					}
				}
			}

			public NBTFile GetFile(int i)
			{
				if(compressedChunks[i] == null) return null;
				using(var chunkStream = Compression.CreateZlibDecompressionStream(compressedChunks[i].compressedChunk))
				{
					return new NBTFile(chunkStream);
				}
			}
		}

		public static Region PreloadRegion(RegionFilePaths filePaths, Dimension parent, GameVersion? worldSaveVersion = null)
		{
			if(!RegionLocation.TryGetFromFileName(filePaths.mainPath, out var loc))
			{
				Logger.Error("Unable to interpret region location from file name: " + filePaths.mainPath);
			}
			Region region = Region.CreateExisting(loc, parent, filePaths);
			region.versionHint = worldSaveVersion;
			return region;
		}

		public static Region LoadRegion(RegionFilePaths filePaths, Dimension parent, GameVersion? worldSaveVersion = null, bool loadChunks = false, bool loadOrphanChunks = false)
		{
			var r = PreloadRegion(filePaths, parent, worldSaveVersion);
			LoadRegionContent(r, loadChunks, loadOrphanChunks);
			return r;
		}

		public static Region LoadMainRegion(string file, Dimension parent, GameVersion? worldSaveVersion = null, bool loadChunks = false, bool loadOrphanChunks = false)
		{
			var paths = new RegionFilePaths(file, null, null);
			return LoadRegion(paths, parent, worldSaveVersion, loadChunks, loadOrphanChunks);
		}

		public static void LoadRegionContent(Region region, bool loadChunks = false, bool loadOrphanChunks = false)
		{
			RegionData main, entities, poi;
			using(var streams = region.sourceFilePaths.OpenStreams(FileMode.Open))
			{
				main = new RegionData(streams.main, region.sourceFilePaths.mainPath);
				entities = streams.entities != null ? new RegionData(streams.entities, region.sourceFilePaths.entitiesPath) : null;
				poi = streams.poi != null ? new RegionData(streams.poi, region.sourceFilePaths.poiPath) : null;
			}
			region.InitializeChunks();
			Parallel.For(0, 1024, WorldForgeManager.ParallelOptions, i =>
			{
				LoadChunk(region, loadChunks, i, main, entities, poi);
			});
		}

		private static void LoadChunk(Region region, bool loadChunks, int i, RegionData main, RegionData entities, RegionData poi)
		{
			if(main.compressedChunks[i] != null)
			{
				var sources = new ChunkSourceData(main.GetFile(i), entities?.GetFile(i), poi?.GetFile(i));
				var coord = new ChunkCoord(i % 32, i / 32);
				region.chunks[coord.x, coord.z] = Chunk.CreateFromNBT(region, coord, sources, region.versionHint, loadChunks);
			}
		}

		public static Region LoadRegionAlphaChunks(string worldSaveDir, RegionLocation location)
		{
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

			var reg = Region.CreateNew(location, null);
			var cs = new ChunkSerializerAlpha(GameVersion.Alpha_1(0));
			Parallel.ForEach(chunkFileLocations, c =>
			{
				var coord = c.Item1;
				//TODO: not sure if path is correct
				var path = c.Item2;
				var regionSpacePos = new ChunkCoord(coord.x & 31, coord.z & 31);
				var chunk = Chunk.CreateFromNBT(reg, regionSpacePos, new ChunkSourceData(new NBTFile(path), null, null));
				reg.chunks[regionSpacePos.x, regionSpacePos.z] = chunk;
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

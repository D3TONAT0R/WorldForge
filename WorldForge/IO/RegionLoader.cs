using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
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

			public readonly int regionX;
			public readonly int regionZ;
			public readonly uint[] locations = new uint[1024];
			public readonly byte[] sizes = new byte[1024];
			public readonly long expectedEOF;

			public readonly CompressedChunkData[] compressedChunks = new CompressedChunkData[1024];

			public RegionData(Stream stream, string filepath)
			{
				string fname = Path.GetFileName(filepath);
				if(Regex.IsMatch(fname, @".*\.-?[0-9]+\.-?[0-9]+\.mc.*"))
				{
					var split = fname.Split('.');
					regionX = int.Parse(split[split.Length - 3]);
					regionZ = int.Parse(split[split.Length - 2]);
				}
				else
				{
					Console.WriteLine("Unable to interpret region location from file name: " + fname);
				}

				for(uint i = 0; i < 1024; i++)
				{
					locations[i] = Read3ByteInt(stream);
					sizes[i] = ReadNext(stream);
				}

				//Ignore timestamps between 4096 and 8192

				expectedEOF = 8192;
				for(int i = 0; i < 1024; i++)
				{
					if(locations[i] > 0)
					{
						stream.Seek(locations[i] * 4096, SeekOrigin.Begin);
						try
						{
							compressedChunks[i] = new CompressedChunkData()
							{
								length = ReadInt(stream),
								compressionType = ReadNext(stream),
								compressedChunk = ReadNext(stream, sizes[i] * 4096 - 5)
							};
						}
						catch
						{
							//A partially generated region may contain a chunk whose stream position is out of range.
							//Can it be safely ignored? NBTExplorer doesn't read that chunk either and it often
							//seems to be located in non-generated areas of the region.

							//Console.WriteLine($"Fail at chunk [{i % 32},{i / 32}]: {e.Message}");
							//throw new FileLoadException($"Failed to load chunk at [{i % 32},{i / 32}].");
						}
						expectedEOF = (locations[i] + sizes[i]) * 4096;
					}
				}
			}
		}

		public static Region LoadRegion(string filepath, GameVersion? worldSaveVersion = null, bool loadChunks = false, bool loadOrphanChunks = false)
		{
			RegionData rd;
			using(var stream = File.Open(filepath, FileMode.Open))
			{
				rd = new RegionData(stream, filepath);
			}
			Region region = new Region(rd.regionX, rd.regionZ, null);
			Parallel.For(0, 1024, WorldForgeManager.ParallelOptions, i =>
			{
				if(rd.compressedChunks[i] != null)
				{
					using(var chunkStream = Compression.CreateZlibDecompressionStream(rd.compressedChunks[i].compressedChunk))
					{
						var coord = new ChunkCoord(i % 32, i / 32);
						int x = i % 32;
						int z = i / 32;
						region.chunks[i % 32, i / 32] = ChunkData.CreateFromNBT(region, coord, new NBTFile(chunkStream), worldSaveVersion, loadChunks);
					}
				}
			});
			return region;
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

			var reg = new Region(location, null);
			var cs = new ChunkSerializerAlpha(GameVersion.Alpha_1(0));
			Parallel.ForEach(chunkFileLocations, c =>
			{
				var coord = c.Item1;
				//TODO: not sure if path is correct
				var path = c.Item2;
				var regionSpacePos = new ChunkCoord(coord.x.Mod(32), coord.z.Mod(32));
				var chunk = ChunkData.CreateFromNBT(reg, regionSpacePos, new NBTFile(path));
				reg.chunks[regionSpacePos.x, regionSpacePos.z] = chunk;
			});
			return reg;
		}

		///<summary>Reads the height data from a region (With [0,0] being the top-left corner).</summary>
		public static short[,] GetHeightmap(string filepath, HeightmapType heightmapType)
		{
			short[,] heightmap = new short[512, 512];
			RegionData rd;
			using(var stream = File.Open(filepath, FileMode.Open))
			{
				rd = new RegionData(stream, filepath);
			}

			Parallel.For(0, 1024, i =>
			{
				if(rd.compressedChunks[i] != null)
				{
					var cd = rd.compressedChunks[i];
					using(var chunkStream = Compression.CreateZlibDecompressionStream(cd.compressedChunk))
					{
						WriteChunkToHeightmap(heightmap, new NBTFile(chunkStream), i % 32, i / 32, heightmapType);
					}
				}
			});
			return heightmap;
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

		private static void WriteChunkToHeightmap(short[,] heightmap, NBTFile nbt, int localChunkX, int localChunkZ, HeightmapType mapType)
		{
			//int chunkDataX = (int)nbt.contents.Get("xPos") - regionPos.x * 32;
			//int chunkDataZ = (int)nbt.contents.Get("zPos") - regionPos.z * 32;
			//var chunkHM = nbt.GetHeightmapFromChunkNBT(mapType);
			try
			{
				//TODO: not global chunk coords
				ChunkSerializer serializer;
				if(nbt.dataVersion.HasValue) serializer = ChunkSerializer.CreateForDataVersion(nbt);
				else serializer = new ChunkSerializerAnvil(GameVersion.Release_1(8));

				var chunk = ChunkData.CreateFromNBT(null, new ChunkCoord(localChunkX, localChunkZ), nbt, null, true);
				//serializer.ReadChunkNBT(chunk, serializer.TargetVersion);
				chunk.WriteToHeightmap(heightmap, localChunkX, localChunkZ, mapType);
				/*for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						byte y = (chunkHM != null) ? (byte)Math.Max(chunkHM[x, z] - 1, 0) : (byte)255;
						if (y > 1)
						{
							while (y > 0 && !Blocks.IsBlockForMap(chunk.GetBlockAt(x, y, z).block, mapType))
							{
								y--;
							}
						}
						heightmap[localChunkX * 16 + x, localChunkZ * 16 + z] = y;
					}
				}
				*/
			}
			catch
			{

			}
		}

		private static byte GetPositive2sComplement(sbyte b)
		{
			unchecked
			{
				return (byte)b;
			}
		}
	}
}

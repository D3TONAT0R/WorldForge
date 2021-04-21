using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using static MCUtils.ChunkData;
using static MCUtils.Blocks;

namespace MCUtils {
	public static class RegionImporter {

		static MemoryStream stream;

		static HeightmapType mapType;
		static World.RegionLocation regionPos;
		static uint[] locations;
		static byte[] sizes;
		static ushort[,] heightmap;
		static byte[][] compressedChunkData;

		///<summary>Creates an instance of <c>Region</c> by reading an existing region file.</summary>
		public static Region OpenRegionFile(string filepath) {
			string fname = Path.GetFileName(filepath);
			int regionX = int.Parse(fname.Split('.')[1]);
			int regionZ = int.Parse(fname.Split('.')[2]);
			using(stream = new MemoryStream()) {
				using(FileStream fs = File.Open(filepath, FileMode.Open)) {
					fs.CopyTo(stream);
				}
				byte[] locationBytes = Read(0, 4096);
				byte[] timestampBytes = Read(4096, 4096);
				locations = new uint[1024];
				sizes = new byte[1024];
				uint lastLocationIndex = 0;
				uint lastLocationValue = 0;
				for(uint i = 0; i < 1024; i++) {
					uint loc = ReadAsInt(locationBytes, i * 4, 3);
					locations[i] = loc;
					byte size = Read(i * 4 + 3, 1)[0];
					sizes[i] = size;
					if(size > 0 && loc > lastLocationValue) {
						lastLocationValue = loc;
						lastLocationIndex = i;
					}
				}
				uint expectedSize = (locations[lastLocationIndex] + sizes[lastLocationIndex]) * 4096;
				Console.WriteLine($"Expected size {expectedSize}, got {stream.Length}, difference {stream.Length - expectedSize}");
				Region r = new Region(regionX, regionZ);
				int misplacedChunks = 0;
				for(int i = 0; i < 1024; i++) {
					if(locations[i] > 0 && sizes[i] > 0) {
						var nbt = new NBTContent(UncompressChunkData(GetChunkData(locations[i] * 4096, out _)), true);
						//int localChunkX = (int)nbt.contents.Get("xPos") - regionX * 32;
						//int localChunkZ = (int)nbt.contents.Get("zPos") - regionZ * 32;
						int localChunkX = i % 32;
						int localChunkZ = i / 32;
						int chunkDataX = (int)nbt.contents.Get("xPos");
						int chunkDataZ = (int)nbt.contents.Get("zPos");
						if(localChunkX + regionX * 32 != chunkDataX || localChunkZ + regionZ * 32 != chunkDataZ) {
							misplacedChunks++;
							if(misplacedChunks <= 10) {
								MCUtilsConsole.WriteWarning($"Chunk location mismatch! Expected[{localChunkX + regionX * 32},{localChunkZ + regionZ * 32}], got [{chunkDataX},{chunkDataZ}]");
							}
							if(misplacedChunks == 11) {
								MCUtilsConsole.WriteWarning("...");
							}
						}
						r.chunks[localChunkX, localChunkZ] = new ChunkData(r, nbt);
					}
				}
				if(misplacedChunks > 5) {
					MCUtilsConsole.WriteWarning($"There are {misplacedChunks} misplaced chunks in total");
				}
				if(stream.Length > expectedSize) {
					Console.WriteLine("Attempting to load orphan chunks ...");
					uint pos = expectedSize;
					try {
						r.orphanChunks = new List<ChunkData>();
						while(pos < stream.Length - 1) {
							var nbt = new NBTContent(UncompressChunkData(GetChunkData(expectedSize, out uint length)), true);
							uint lengthKiB = (uint)Math.Ceiling(length / 4096f);
							r.orphanChunks.Add(new ChunkData(r, nbt));
							Console.WriteLine($"Orphan chunk found!");
							pos += lengthKiB * 4096;
						}
					} catch {
						Console.WriteLine("Failed to load remaining orphan chunk data.");
					}
				}
				return r;
			}
		}

		///<summary>Loads chunk data from a specified byte offset in a region file (for debugging purposes)</summary>
		public static NBTContent LoadChunkDataFromOffset(string filepath, uint byteOffset) {
			using(stream = new MemoryStream()) {
				using(FileStream fs = File.Open(filepath, FileMode.Open)) {

					fs.CopyTo(stream);
				}
				stream.Position = byteOffset;
				return new NBTContent(UncompressChunkData(GetChunkData(byteOffset, out _)), true);
			}
		}

		///<summary>Reads the height data from a region (With [0,0] being the top-left corner).</summary>
		public static ushort[,] GetHeightmap(string filepath, HeightmapType heightmapType) {
			mapType = heightmapType;
			string fname = Path.GetFileName(filepath);
			int regionX = int.Parse(fname.Split('.')[1]);
			int regionZ = int.Parse(fname.Split('.')[2]);
			regionPos = new World.RegionLocation(regionX, regionZ);
			using(stream = new MemoryStream()) {
				using(FileStream fs = File.Open(filepath, FileMode.Open)) {
					stream = new MemoryStream();
					fs.CopyTo(stream);
				}
				byte[] locationBytes = Read(0, 4096);
				byte[] timestampBytes = Read(4096, 4096);
				locations = new uint[1024];
				sizes = new byte[1024];
				for(uint i = 0; i < 1024; i++) {
					locations[i] = ReadAsInt(locationBytes, i * 4, 3);
					sizes[i] = Read(i * 4 + 3, 1)[0];
				}
				compressedChunkData = new byte[1024][];
				for(int i = 0; i < 1024; i++) {
					if(locations[i] > 0 && sizes[i] > 0) {
						compressedChunkData[i] = GetChunkData(locations[i] * 4096, out _);
					}
				}
				heightmap = new ushort[512, 512];
				Parallel.For(0, 1024, GetChunkHeightmap);
				return heightmap;
			}
		}

		public static Bitmap GetSurfaceMap(string filepath, HeightmapType surfaceType) {
			Region r = OpenRegionFile(filepath);
			var hm = r.GetHeightmapFromNBT(surfaceType);
			for(int z = 0; z < 512; z++) {
				for(int x = 0; x < 512; x++) {
					short y = hm[x, z];
					while(!IsBlockForMap(r.GetBlockState(x, y, z), surfaceType) && y > 0) {
						y--;
					}
					hm[x, z] = y;
				}
			}
			Bitmap bmp = new Bitmap(512, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			for(int z = 0; z < 512; z++) {
				for(int x = 0; x < 512; x++) {
					string block = r.GetBlock(x, hm[x, z], z);
					if(block == "minecraft:air") block = r.GetBlock(x, hm[x, z] - 1, z);
					bmp.SetPixel(x, z, GetMapColor(block));
				}
			}
			return bmp;
		}

		private static void GetChunkHeightmap(int i) {
			if(compressedChunkData[i] != null) {
				var nbt = new NBTContent(UncompressChunkData(compressedChunkData[i]), true);
				int localChunkX = i % 32;
				int localChunkZ = i / 32;
				//int chunkDataX = (int)nbt.contents.Get("xPos") - regionPos.x * 32;
				//int chunkDataZ = (int)nbt.contents.Get("zPos") - regionPos.z * 32;
				var chunkHM = nbt.GetHeightmapFromChunkNBT(mapType);
				ChunkData chunk = new ChunkData(null, nbt);
				for(int x = 0; x < 16; x++) {
					for(int z = 0; z < 16; z++) {
						byte y = (chunkHM != null) ? (byte)Math.Max(chunkHM[x, z] - 1, 0) : (byte)255;
						if(y > 1) {
							while(y > 0 && !IsBlockForMap(chunk.GetBlockAt(x, y, z), mapType)) {
								y--;
							}
						}
						heightmap[localChunkX * 16 + x, localChunkZ * 16 + z] = y;
					}
				}
			}
		}

		private static byte[] Read(uint start, uint length) {
			byte[] buffer = new byte[length];
			stream.Position = start;
			for(int i = 0; i < length; i++) {
				int result = stream.ReadByte();
				if(result >= 0) {
					buffer[i] = (byte)result;
				} else {
					buffer[i] = 0;
					throw new EndOfStreamException();
				}
			}
			return buffer;
		}

		private static uint ReadAsInt(byte[] arr, uint start, int length) {
			byte[] bytes = new byte[4];
			int padding = 4 - length;
			for(int i = 0; i < length; i++) bytes[i + padding] = arr[start + i];
			if(BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToUInt32(bytes, 0);
		}

		private static byte[] GetChunkData(uint pos, out uint length) {
			byte[] bytes = new byte[4];
			stream.Position = pos;
			for(int i = 0; i < 4; i++) {
				bytes[i] = (byte)stream.ReadByte();
			}
			length = ReadAsInt(bytes, 0, 4);
			return Read(pos + 5, length);
		}

		private static byte[] UncompressChunkData(byte[] compressed) {
			return ZlibStream.UncompressBuffer(compressed);
		}
	}
}
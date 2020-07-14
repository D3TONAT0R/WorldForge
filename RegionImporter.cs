using System.IO;
using System;
using Ionic.Zlib;

namespace MCUtils {
	public static class RegionImporter {

		static MemoryStream stream;

		public static Region OpenRegionFile(string filepath) {
			string fname = Path.GetFileName(filepath);
			int regionX = int.Parse(fname.Split('.')[1]);
			int regionZ = int.Parse(fname.Split('.')[2]);
			FileStream fs = File.Open(filepath, FileMode.Open);
			stream = new MemoryStream();
			fs.CopyTo(stream);
			byte[] locationBytes = Read(0, 4096);
			byte[] timestampBytes = Read(4096, 4096);
			uint[] locations = new uint[1024];
			byte[] sizes = new byte[1024];
			for(uint i = 0; i < 1024; i++) {
				locations[i] = ReadAsInt(locationBytes, i*4, 3);
				sizes[i] = Read(i*4+3, 1)[0];
			}
			ushort[,] hm = new ushort[512, 512];
			Region r = new Region();
			for(int i = 0; i < 1024; i++) {
				if(locations[i] > 0 && sizes[i] > 0) {
					var nbt = new NBTContent(GetChunkData(locations[i], sizes[i]));
					int localChunkX = (int)nbt.contents.Get("xPos") - regionX * 32;
					int localChunkZ = (int)nbt.contents.Get("zPos") - regionZ * 32;
					r.chunks[localChunkX,localChunkZ] = new ChunkData(nbt);
				}
			}
			stream.Close();
			fs.Close();
			return r;
		}

		private static byte[] Read(uint start, int length) {
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
			for(int i = 0; i < length; i++) bytes[i+ padding] = arr[start+i];
			if(BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToUInt32(bytes);
		}

		private static byte[] GetChunkData(uint loc, byte size) {
			loc *= 4096;
			int length = size*4096;
			byte[] compressed = Read(loc+5, length);
			return ZlibStream.UncompressBuffer(compressed);
		}
	}
}
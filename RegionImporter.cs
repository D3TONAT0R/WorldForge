using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using static MCUtils.Blocks;

namespace MCUtils {
	public class RegionImporter {

		MemoryStream stream;
		
		HeightmapType mapType;
		World.RegionLocation regionPos;
		uint[] locations;
		byte[] sizes;
		ushort[,] heightmap;
		byte[][] compressedChunkData;

		public bool allowOrphanChunkLoad = false;

		private Region region;

		private RegionImporter()
		{

		}

		private RegionImporter(string filepath)
		{
			string fname = Path.GetFileName(filepath);
			int regionX = int.Parse(fname.Split('.')[1]);
			int regionZ = int.Parse(fname.Split('.')[2]);
			using (stream = new MemoryStream())
			{
				using (FileStream fs = File.Open(filepath, FileMode.Open))
				{
					fs.CopyTo(stream);
				}
				byte[] locationBytes = Read(0, 4096);
				byte[] timestampBytes = Read(4096, 4096);
				locations = new uint[1024];
				sizes = new byte[1024];
				uint lastLocationIndex = 0;
				uint lastLocationValue = 0;
				for (uint i = 0; i < 1024; i++)
				{
					uint loc = ReadAsInt(locationBytes, i * 4, 3);
					locations[i] = loc;
					byte size = Read(i * 4 + 3, 1)[0];
					sizes[i] = size;
					if (size > 0 && loc > lastLocationValue)
					{
						lastLocationValue = loc;
						lastLocationIndex = i;
					}
				}
				uint expectedSize = (locations[lastLocationIndex] + sizes[lastLocationIndex]) * 4096;
				Console.WriteLine($"Expected size {expectedSize}, got {stream.Length}, difference {stream.Length - expectedSize}");
				region = new Region(regionX, regionZ);
				int misplacedChunks = 0;
				for (int i = 0; i < 1024; i++)
				{
					if (locations[i] > 0 && sizes[i] > 0)
					{
						var nbt = new NBTContent(UncompressChunkData(GetChunkData(locations[i] * 4096, out _)), true);
						//int localChunkX = (int)nbt.contents.Get("xPos") - regionX * 32;
						//int localChunkZ = (int)nbt.contents.Get("zPos") - regionZ * 32;
						int localChunkX = i % 32;
						int localChunkZ = i / 32;
						int chunkDataX = (int)nbt.contents.Get("xPos");
						int chunkDataZ = (int)nbt.contents.Get("zPos");
						if (localChunkX + regionX * 32 != chunkDataX || localChunkZ + regionZ * 32 != chunkDataZ)
						{
							misplacedChunks++;
							if (misplacedChunks <= 10)
							{
								MCUtilsConsole.WriteWarning($"Chunk location mismatch! Expected[{localChunkX + regionX * 32},{localChunkZ + regionZ * 32}], got [{chunkDataX},{chunkDataZ}]");
							}
							if (misplacedChunks == 11)
							{
								MCUtilsConsole.WriteWarning("...");
							}
						}
						region.chunks[localChunkX, localChunkZ] = new ChunkData(region, nbt);
					}
				}
				if (misplacedChunks > 5)
				{
					MCUtilsConsole.WriteWarning($"There are {misplacedChunks} misplaced chunks in total");
				}
				if (allowOrphanChunkLoad)
				{
					if (stream.Length > expectedSize)
					{
						Console.WriteLine("Attempting to load orphan chunks ...");
						uint pos = expectedSize;
						try
						{
							region.orphanChunks = new List<ChunkData>();
							while (pos < stream.Length - 1)
							{
								var nbt = new NBTContent(UncompressChunkData(GetChunkData(expectedSize, out uint length)), true);
								uint lengthKiB = (uint)Math.Ceiling(length / 4096f);
								region.orphanChunks.Add(new ChunkData(region, nbt));
								Console.WriteLine($"Orphan chunk found!");
								pos += lengthKiB * 4096;
							}
						}
						catch
						{
							Console.WriteLine("Failed to load remaining orphan chunk data.");
						}
					}
				}
			}
		}

		///<summary>Creates an instance of <c>Region</c> by reading an existing region file.</summary>
		public static Region OpenRegionFile(string filepath) {
			return new RegionImporter(filepath).region;
		}

		///<summary>Loads chunk data from a specified byte offset in a region file (for debugging purposes)</summary>
		public static NBTContent LoadChunkDataFromOffset(string filepath, uint byteOffset) {
			var ri = new RegionImporter();
			using(ri.stream = new MemoryStream()) {
				using(FileStream fs = File.Open(filepath, FileMode.Open)) {

					fs.CopyTo(ri.stream);
				}
				ri.stream.Position = byteOffset;
				return new NBTContent(UncompressChunkData(ri.GetChunkData(byteOffset, out _)), true);
			}
		}

		///<summary>Reads the height data from a region (With [0,0] being the top-left corner).</summary>
		public static ushort[,] GetHeightmap(string filepath, HeightmapType heightmapType) {
			var ri = new RegionImporter();
			ri.mapType = heightmapType;
			int regionX = 0;
			int regionZ = 0;
			string fname = Path.GetFileNameWithoutExtension(filepath);
			var split = fname.Split('.');
			if(split.Length > 2) {
				regionX = int.Parse(fname.Split('.')[1]);
				regionZ = int.Parse(fname.Split('.')[2]);
			}
			ri.regionPos = new World.RegionLocation(regionX, regionZ);
			using(ri.stream = new MemoryStream()) {
				using(FileStream fs = File.Open(filepath, FileMode.Open)) {
					ri.stream = new MemoryStream();
					fs.CopyTo(ri.stream);
				}
				byte[] locationBytes = ri.Read(0, 4096);
				byte[] timestampBytes = ri.Read(4096, 4096);
				ri.locations = new uint[1024];
				ri.sizes = new byte[1024];
				for(uint i = 0; i < 1024; i++) {
					ri.locations[i] = ReadAsInt(locationBytes, i * 4, 3);
					ri.sizes[i] = ri.Read(i * 4 + 3, 1)[0];
				}
				ri.compressedChunkData = new byte[1024][];
				for(int i = 0; i < 1024; i++) {
					if(ri.locations[i] > 0 && ri.sizes[i] > 0) {
						ri.compressedChunkData[i] = ri.GetChunkData(ri.locations[i] * 4096, out _);
					}
				}
				ri.heightmap = new ushort[512, 512];
				Parallel.For(0, 1024, ri.GetChunkHeightmap);
				return ri.heightmap;
			}
		}

		public static Bitmap GetSurfaceMap(string filepath, HeightmapType surfaceType, bool mcMapShading) {
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
					int y = hm[x, z];
					string block = r.GetBlock(x, y, z);
					if (block == "minecraft:air" && y > 0)
					{
						throw new ArgumentException("the mapped block was air");
					}
					int shade = 0;
					
					if(mcMapShading && z > 0)
					{
						if (block == "minecraft:water")
						{
							//Water dithering
							var depth = r.GetWaterDepth(x, y, z);
							if (depth < 8) shade = 1;
							else if (depth < 16) shade = 0;
							else shade = -1;
							if(depth%8 >= 4 && shade > -1)
							{
								if (x % 2 == z % 2) shade--;
							}
						}
						else
						{
							int above = hm[x, z - 1];
							if (above > y) shade = -1;
							else if (above < y) shade = 1;
						}
					}
					bmp.SetPixel(x, z, GetMapColor(block, shade));
				}
			}
			return bmp;
		}

		private void GetChunkHeightmap(int i) {
			if(compressedChunkData[i] != null) {
				var nbt = new NBTContent(UncompressChunkData(compressedChunkData[i]), true);
				int localChunkX = i % 32;
				int localChunkZ = i / 32;
				//int chunkDataX = (int)nbt.contents.Get("xPos") - regionPos.x * 32;
				//int chunkDataZ = (int)nbt.contents.Get("zPos") - regionPos.z * 32;
				var chunkHM = nbt.GetHeightmapFromChunkNBT(mapType);
				try
				{
					ChunkData chunk = new ChunkData(null, nbt);
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							byte y = (chunkHM != null) ? (byte)Math.Max(chunkHM[x, z] - 1, 0) : (byte)255;
							if (y > 1)
							{
								while (y > 0 && !IsBlockForMap(chunk.GetBlockAt(x, y, z), mapType))
								{
									y--;
								}
							}
							heightmap[localChunkX * 16 + x, localChunkZ * 16 + z] = y;
						}
					}
				}
				catch
				{

				}
			}
		}

		private byte[] Read(uint start, uint length) {
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

		private byte[] GetChunkData(uint pos, out uint length) {
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

using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using static MCUtils.ChunkData;
using static MCUtils.NBTContent;

namespace MCUtils {
	public class Region {

		public byte[,] heightmap;
		public ChunkData[,] chunks;

		public int[,,] finalBiomeData;

		public Region() {
			chunks = new ChunkData[32, 32];
		}

		///<summary>Returns true if the given locations contains air or the section has not been generated yet</summary>
		public bool IsAir(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			return b == null || b == "minecraft:air";
		}

		///<summary>Is the location within the region's bounds?</summary>
		public bool IsWithinBoundaries(int x, int y, int z) {
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return false;
			else return true;
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			if(b == null) return false;
			return b == World.defaultBlock;
		}

		///<summary>Gets the block type at the given location.</summary>
		public string GetBlock(int x, int y, int z) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return null;
			if(chunks[chunkX, chunkZ] != null) {
				var b = chunks[chunkX, chunkZ].GetBlockAt(x % 16, y, z % 16);
				return b != null ? b.block : "minecraft:air";
			} else {
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(int x, int y, int z) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return null;
			if(chunks[chunkX, chunkZ] != null) {
				return chunks[chunkX, chunkZ].GetBlockAt(x % 16, y, z % 16);
			} else {
				return null;
			}
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(int x, int y, int z, string block) {
			return SetBlock(x, y, z, new BlockState(block));
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(int x, int y, int z, BlockState block) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return false;
			if(chunks[chunkX, chunkZ] == null) {
				chunks[chunkX, chunkZ] = new ChunkData("minecraft:stone");
			}
			chunks[chunkX, chunkZ].SetBlockAt(x % 16, y, z % 16, block);
			return true;
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(int x, int y, int z) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			chunks[chunkX, chunkZ].SetDefaultBlockAt(x % 16, y, z % 16);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, byte biome) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return;
			if(chunks[chunkX, chunkZ] != null) {
				for(int y = 0; y < 256; y++) {
					chunks[chunkX, chunkZ].SetBiomeAt(x % 16, z % 16, biome);
				}
			}
		}

		///<summary>Generates a heightmap by reading the chunk's heightmaps or calculating it from existing blocks.</summary>
		public ushort[,] GetHeightmap() {
			ushort[,] hm = new ushort[512, 512];
			for(int x = 0; x < 32; x++) {
				for(int z = 0; z < 32; z++) {
					var c = chunks[x, z];
					if(c != null) {
						c.WriteToHeightmap(hm, x, z);
					}
				}
			}
			return hm;
		}

		///<summary>Generates a full .mca file stream for use in Minecraft.</summary>
		public void WriteRegionToStream(FileStream stream, int regionX, int regionZ) {
			DateTime time = System.DateTime.Now;
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 8192; i++) {
				stream.WriteByte(0);
			}
			for(int z = 0; z < 32; z++) {
				for(int x = 0; x < 32; x++) {
					int i = z * 32 + x;
					locations[i] = (int)(stream.Position / 4096);
					var chunkData = MakeCompoundForChunk(chunks[x, z], 32 * regionX + x, 32 * regionZ + z);
					List<byte> bytes = new List<byte>();
					chunkData.WriteToBytes(bytes);
					byte[] compressed = ZlibStream.CompressBuffer(bytes.ToArray());
					stream.Write(Converter.ReverseEndianness(BitConverter.GetBytes(compressed.Length)));
					stream.WriteByte(2);
					stream.Write(compressed);
					var paddingMod = stream.Length % 4096;
					//Pad the data to the next 4096 bytes
					if(paddingMod > 0) {
						byte[] padding = new byte[4096 - paddingMod];
						stream.Write(padding);
					}
					sizes[i] = (byte)((int)(stream.Position / 4096) - locations[i]);
				}
				Program.WriteProgress(string.Format("Writing chunks to stream [{0}/{1}]", z * 32, 1024), (z * 32f) / 1024f);
			}
			stream.Position = 0;
			for(int i = 0; i < 1024; i++) {
				byte[] offsetBytes = Converter.ReverseEndianness(BitConverter.GetBytes(locations[i]));
				stream.WriteByte(offsetBytes[1]);
				stream.WriteByte(offsetBytes[2]);
				stream.WriteByte(offsetBytes[3]);
				stream.WriteByte(sizes[i]);
			}
			DateTime time2 = System.DateTime.Now;
			TimeSpan len = time2.Subtract(time);
			Program.WriteLine("Generating MCA took " + Math.Round(len.TotalSeconds * 100f) / 100f + "s");
		}

		private NBTContent MakeCompoundForChunk(ChunkData chunk, int chunkX, int chunkZ) {
			var nbt = new NBTContent();
			nbt.dataVersion = 2504; //1.16 version ID
			nbt.contents.Add("xPos", chunkX);
			nbt.contents.Add("zPos", chunkZ);
			nbt.contents.Add("Status", "light");
			ListContainer sections = new ListContainer(NBTTag.TAG_Compound);
			nbt.contents.Add("Sections", sections);
			chunk.WriteToNBT(nbt.contents, true);
			//Add the rest of the tags and leave them empty
			nbt.contents.Add("Heightmaps", new CompoundContainer());
			nbt.contents.Add("Structures", new CompoundContainer());
			nbt.contents.Add("Entities", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("LiquidTicks", new ListContainer(NBTTag.TAG_End));
			ListContainer postprocessing = new ListContainer(NBTTag.TAG_List);
			for(int i = 0; i < 16; i++) postprocessing.Add("", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("PostProcessing", postprocessing);
			nbt.contents.Add("TileEntities", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("TileTicks", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("InhabitedTime", 0L);
			nbt.contents.Add("LastUpdate", 0L);
			return nbt;
		}
	}
}
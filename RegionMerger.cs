using System.IO;
using System.Drawing;
using System;
using static MCUtils.ChunkData;

namespace MCUtils {
	public class RegionMerger : IUtilTask {

		public Region mergedRegion;

		public void Run() {
			Program.WriteLine("Enter path to region file 1:");
			string r1 = GetFilePath(false);
			if(r1 == null) return;
			Program.WriteLine("Enter path to region file 2:");
			string r2 = GetFilePath(false);
			if(r2 == null) return;
			Program.WriteLine("Enter path to map image:");
			string map = GetFilePath(false);
			if(map == null) return;
			Program.WriteLine("Enter path to output file:");
			string savepath = GetFilePath(true);
			Program.WriteLine("Starting merge...");
			Region region1;
			Region region2;
			try {
				region1 = RegionImporter.OpenRegionFile(r1);
				region2 = RegionImporter.OpenRegionFile(r2);
			} catch(Exception e) {
				Program.WriteError("Failed to open region file(s):");
				Program.WriteError(e.ToString());
				return;
			}
			Bitmap mask = new Bitmap(map);
			bool perBlockMask = mask.Width == 512 && mask.Height == 512;
			bool perChunkMask = mask.Width == 32 && mask.Height == 32;
			if(!perBlockMask && !perChunkMask) {
				Program.WriteError("The mask image was not the correct size. It must be either 512x512 or 32x32!");
				return;
			}
			MergeRegions(region1, region2, mask);
			Program.WriteLine("Writing file...");
			FileStream stream = new FileStream(savepath, FileMode.Create);
			mergedRegion.WriteRegionToStream(stream, 0, 0);
			stream.Close();
			Program.WriteLine("Done");
		}

		private void MergeRegions(Region r1, Region r2, Bitmap mask) {
			mergedRegion = new Region();
			bool perChunkMask = mask.Width == 32;
			byte[,] blockMergeMap = new byte[512,512];
			byte[,] chunkMergeMap = new byte[32,32];
			if(perChunkMask) {
				for(int x = 0; x < 32; x++) {
					for(int z = 0; z < 32; z++) {
						chunkMergeMap[x,z] = (mask.GetPixel(x,31-z).GetBrightness() > 0.5f) ? (byte)2 : (byte)1;
					}
				}
			} else {
				for(int x = 0; x < 512; x++) {
					for(int z = 0; z < 512; z++) {
						blockMergeMap[x,z] = (mask.GetPixel(x,511-z).GetBrightness() > 0.5f) ? (byte)1 : (byte)0;
					}
				}
				for(int x = 0; x < 32; x++) {
					for(int z = 0; z < 32; z++) {
						int mergemapTotal = 0;
						for(int x1 = 0; x1 < 16; x1++) {
							for(int z1 = 0; z1 < 16; z1++) {
								mergemapTotal += blockMergeMap[x*16+x1,z*16+z1] == 1 ? 1 : -1;
							}
						}
						if(mergemapTotal == -256) {
							//All values refer to region 1
							chunkMergeMap[x,z] = 1;
						} else if(mergemapTotal == 256) {
							//All values refer to region 2
							chunkMergeMap[x,z] = 2;
						} else {
							//The values refer to both chunks, per-block merging must be performed here
							chunkMergeMap[x,z] = 0;
						}
					}
				}
			}
			//Merge full chunks first, then move on to the mixed chunks
			MergeChunks(r1, r2, chunkMergeMap);
			if(!perChunkMask){
				MergeBlockColumns(r1, r2, chunkMergeMap, blockMergeMap);
			}
		}

		private void MergeChunks(Region r1, Region r2, byte[,] chunkMergeMap) {
			for(int x = 0; x < 32; x++) {
				for(int z = 0; z < 32; z++) {
					byte b = chunkMergeMap[x,z];
					if(b != 0) {
						var sourceRegion = b == 1 ? r1 : r2;
						CopyChunk(mergedRegion, sourceRegion, x, z);
					}
				}
			}
		}

		private void MergeBlockColumns(Region r1, Region r2, byte[,] chunkMergeMap, byte [,] blockMergeMap) {
			for(int cx = 0; cx < 32; cx++) {
				for(int cz = 0; cz < 32; cz++) {
					if(chunkMergeMap[cx, cz] == 0) {
						//This chunk was not merged using MergeChunks, do it now 
						for(int x = 0; x < 16; x++) {
							for(int z = 0; z < 16; z++) {
								int gx = cx*16+x;
								int gz = cz*16+z;
								var sourceRegion = blockMergeMap[gx,gz] == 1 ? r2 : r1;
								CopyBlockColumn(mergedRegion, sourceRegion, gx, gz);
							}
						}
					}
				}
			}
		}

		private void CopyBlockColumn(Region dst, Region source, int x, int z) {
			for(int y = 0; y < 256; y++) {
				BlockState block = source.GetBlockState(x,y,z);
				if(block != null) {
					dst.SetBlock(x, y, z, block);
				}
			}
		}

		private void CopyChunk(Region dst, Region source, int chunkX, int chunkZ) {
			dst.chunks[chunkX, chunkZ] = source.chunks[chunkX, chunkZ];
			var chunk = dst.chunks[chunkX, chunkZ];
			chunk.sourceNBT.contents.Add("xPos", chunkX);
			chunk.sourceNBT.contents.Add("zPos", chunkZ);
		}

		private string GetFilePath(bool isSaveLocation) {
			bool exit = false;
			while(!exit) {
				string file = Program.GetInput();
				if(file.StartsWith("exit")) break;
				if(!isSaveLocation) {
					if(File.Exists(file)) {
						return file;
					} else {
						Program.WriteWarning("Path is invalid. Try again.");
					}
				} else {
					if(Directory.Exists(Path.GetDirectoryName(file))) {
						return file;
					} else {
						Program.WriteWarning("Directory does not exist. Try again.");
					}
				}
			}
			return null;
		}
	}
}
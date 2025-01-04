using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using WorldForge.Coordinates;

namespace WorldForge.Regions
{
	public class RegionMerger
	{
		public enum MergeAction
		{
			Ignore,
			FromRegion1,
			FromRegion2,
			Merge
		}

		private Region region1;
		private Region region2;

		private byte[,] chunkMask;
		private byte[,] blockMask;

		//public MergeAction mergeTileEntities = MergeAction.Merge;
		public MergeAction mergeEntities = MergeAction.Merge;
		//public MergeAction mergeBiomes = MergeAction.Merge;

		private RegionMerger(Region r1, Region r2)
		{
			region1 = r1;
			region2 = r2;
		}

		public RegionMerger(Region r1, Region r2, byte[,] mask) : this(r1, r2)
		{
			if (mask.GetLength(0) == 32 && mask.GetLength(1) == 32)
			{
				chunkMask = mask;
			}
			else if (mask.GetLength(0) == 512 && mask.GetLength(1) == 512)
			{
				blockMask = mask;
				chunkMask = BuildChunkMask(mask);
			}
			else
			{
				throw new ArgumentException("Masks must be either 512x512 or 32x32");
			}
		}

		public RegionMerger(Region r1, Region r2, Image<Rgba32> mask) : this(r1, r2, ConvertToMergeMask(mask))
		{
		}

		public RegionMerger(Region r1, Region r2, bool[,] mask) : this(r1, r2, ConvertToMergeMask(mask))
		{
		}

		public static byte[,] ConvertToMergeMask(Image<Rgba32> mask)
		{
			if (!(mask.Width == 512 && mask.Height == 512) && !(mask.Width == 32 && mask.Height == 32))
			{
				throw new ArgumentException("Bitmap masks must be either 512x512 or 32x32");
			}
			byte[,] map = new byte[mask.Width, mask.Height];
			for (int x = 0; x < mask.Width; x++)
			{
				for (int z = 0; z < mask.Height; z++)
				{
					map[x, z] = (byte)(mask[x, z].R > 0.5f ? 2 : 1);
				}
			}
			return map;
		}

		public static byte[,] ConvertToMergeMask(bool[,] mask)
		{
			if (!(mask.GetLength(0) == 512 && mask.GetLength(1) == 512) && !(mask.GetLength(0) == 32 && mask.GetLength(1) == 32))
			{
				throw new ArgumentException("Bitmap masks must be either 512x512 or 32x32");
			}
			byte[,] map = new byte[mask.GetLength(0), mask.GetLength(1)];
			for (int x = 0; x < mask.GetLength(0); x++)
			{
				for (int z = 0; z < mask.GetLength(1); z++)
				{
					map[x, z] = (byte)(mask[x, z] ? 2 : 1);
				}
			}
			return map;
		}

		private byte[,] BuildChunkMask(byte[,] blockMask)
		{
			byte[,] cm = new byte[32, 32];
			for (int x = 0; x < 32; x++)
			{
				for (int z = 0; z < 32; z++)
				{
					int mergemapTotal = 0;
					for (int x1 = 0; x1 < 16; x1++)
					{
						for (int z1 = 0; z1 < 16; z1++)
						{
							byte b = blockMask[x * 16 + x1, z * 16 + z1];
							if (b == 2)
							{
								mergemapTotal++;
							}
							else if (b == 1)
							{
								mergemapTotal--;
							}
						}
					}
					if (mergemapTotal == -256)
					{
						//All values refer to region 1
						cm[x, z] = 1;
					}
					else if (mergemapTotal == 256)
					{
						//All values refer to region 2
						cm[x, z] = 2;
					}
					else
					{
						//The values refer to both chunks, only per-block merging can be performed here
						cm[x, z] = 0;
					}
				}
			}
			return cm;
		}

		public Region Merge()
		{
			var merged = Region.CreateNew(region1.regionPos, null);
			//Merge full chunks first, then move on to single blocks
			if (chunkMask != null)
			{
				MergeChunks(merged);
			}
			if (blockMask != null)
			{
				MergeBlockColumns(merged);
			}
			return merged;
		}

		void MergeChunks(Region mergedRegion)
		{
			for (int x = 0; x < 32; x++)
			{
				for (int z = 0; z < 32; z++)
				{
					var sourceRegion = GetSourceRegion(chunkMask[x, z]);
					if (sourceRegion != null)
					{
						CopyChunk(mergedRegion, sourceRegion, x, z);
						if (mergeEntities == MergeAction.Ignore)
						{
							mergedRegion.chunks[x, z].Entities.Clear();
						}
						else if (mergeEntities == MergeAction.FromRegion1)
						{
							mergedRegion.chunks[x, z].Entities.Clear();
							if(region1.chunks[x, z] != null)
							{
								mergedRegion.chunks[x, z].Entities.AddRange(region1.chunks[x, z].Entities);
							}
						}
						else if (mergeEntities == MergeAction.FromRegion2)
						{
							mergedRegion.chunks[x, z].Entities.Clear();
							if(region2.chunks[x, z] != null)
							{
								mergedRegion.chunks[x, z].Entities.AddRange(region2.chunks[x, z].Entities);
							}
						}
						else if (mergeEntities == MergeAction.Merge)
						{
							mergedRegion.chunks[x, z].Entities.Clear();
							if(sourceRegion.chunks[x, z] != null)
							{
								mergedRegion.chunks[x, z].Entities.AddRange(sourceRegion.chunks[x, z].Entities);
							}
						}
					}
				}
			}
		}

		void MergeBlockColumns(Region mergedRegion)
		{
			for (int cx = 0; cx < 32; cx++)
			{
				for (int cz = 0; cz < 32; cz++)
				{
					if (chunkMask == null || chunkMask[cx, cz] == 0)
					{
						//This chunk was not merged using MergeChunks, do it now instead
						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								int gx = cx * 16 + x;
								int gz = cz * 16 + z;
								var sourceRegion = GetSourceRegion(blockMask[gx, gz]);
								if (sourceRegion != null)
								{
									CopyBlockColumn(mergedRegion, sourceRegion, gx, gz);
									//Also copy any entities contained within this column
									if (mergeEntities == MergeAction.Ignore)
									{
										//Do nothing
									}
									else if (mergeEntities == MergeAction.FromRegion1)
									{
										CopyEntitiesFromSourceColumn(mergedRegion, region1, cx, cz, x, z);
									}
									else if (mergeEntities == MergeAction.FromRegion2)
									{
										CopyEntitiesFromSourceColumn(mergedRegion, region2, cx, cz, x, z);
									}
									else if (mergeEntities == MergeAction.Merge)
									{
										CopyEntitiesFromSourceColumn(mergedRegion, sourceRegion, cx, cz, x, z);
									}
								}
							}
						}
					}
				}
			}
		}

		static void CopyBlockColumn(Region dst, Region source, int x, int z)
		{
			for (int y = 0; y < 256; y++)
			{
				BlockCoord pos = (x, y, z);
				BlockState block = source.GetBlockState(pos);
				if (block != null)
				{
					dst.SetBlock(pos, block);
					//Also copy any tile entities associated with this block
					var te = source.GetTileEntity(pos);
					if (te != null)
					{
						dst.SetTileEntity(pos, te);
					}
				}
			}
		}

		void CopyChunk(Region dst, Region source, int chunkX, int chunkZ)
		{
			dst.chunks[chunkX, chunkZ] = source.chunks[chunkX, chunkZ];
			//var chunk = dst.chunks[chunkX, chunkZ];
			//chunk.sourceNBT.contents.Add("xPos", chunkX);
			//chunk.sourceNBT.contents.Add("zPos", chunkZ);
		}

		void CopyEntitiesFromSourceColumn(Region merged, Region sourceRegion, int cx, int cz, int x, int z)
		{
			var chunk = sourceRegion.chunks[cx, cz];
			if (chunk != null && chunk.Entities != null)
			{
				foreach (var e in chunk.Entities)
				{
					if ((e.BlockPosX & 511) == 16 * cx + x && (e.BlockPosZ & 511) == 16 * cz + z)
					{
						merged.chunks[cx, cz].Entities.Add(e);
					}
				}
			}
		}

		private Region GetSourceRegion(byte index)
		{
			if (index == 1)
			{
				return region1;
			}
			else if (index == 2)
			{
				return region2;
			}
			else
			{
				return null;
			}
		}
	}
}
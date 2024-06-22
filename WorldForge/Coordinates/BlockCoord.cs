using System;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.Coordinates
{
	public struct BlockCoord : INBTConverter
	{
		public int x;
		public int y;
		public int z;

		public RegionLocation Region => new RegionLocation((int)Math.Floor(x / 512f), (int)Math.Floor(z / 512f));

		public ChunkCoord Chunk => new ChunkCoord((int)Math.Floor(x / 16f), (int)Math.Floor(z / 16f));

		public BlockCoord LocalRegionCoords => new BlockCoord(x.Mod(512), y, z.Mod(512));

		public BlockCoord LocalChunkCoords => new BlockCoord(x.Mod(16), y, z.Mod(16));

		public BlockCoord LocalSectionCoords => new BlockCoord(x.Mod(16), y.Mod(16), z.Mod(16));

		public BlockCoord Below => new BlockCoord(x, y - 1, z);
		public BlockCoord Above => new BlockCoord(x, y + 1, z);
		public BlockCoord Left => new BlockCoord(x - 1, y, z);
		public BlockCoord Right => new BlockCoord(x + 1, y, z);
		public BlockCoord Back => new BlockCoord(x, y, z - 1);
		public BlockCoord Forward => new BlockCoord(x, y, z + 1);

		public BlockCoord(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public override string ToString()
		{
			return $"[{x},{y},{z}]";
		}

		public static BlockCoord operator +(BlockCoord l, BlockCoord r)
		{
			return new BlockCoord(l.x + r.x, l.y + r.y, l.z + r.z);
		}

		public static BlockCoord operator -(BlockCoord l, BlockCoord r)
		{
			return new BlockCoord(l.x - r.x, l.y - r.y, l.z - r.z);
		}

		public static implicit operator BlockCoord((int, int, int) tuple)
		{
			return new BlockCoord(tuple.Item1, tuple.Item2, tuple.Item3);
		}

		public BlockCoord ChunkToRegionSpace(ChunkCoord localChunk)
		{
			if(localChunk.x > 31 || localChunk.z > 31 || localChunk.x < 0 || localChunk.z < 0)
			{
				throw new ArgumentException("Chunk coordinate was not in region space: " + localChunk);
			}
			return this + localChunk.BlockCoord;
		}

		public BlockCoord RegionToWorldSpace(RegionLocation localRegion)
		{
			return this + localRegion.GetBlockCoord(0, 0, 0);
		}

		public BlockCoord ChunkToWorldSpace(ChunkCoord localChunk, RegionLocation localRegion)
		{
			return ChunkToRegionSpace(localChunk).RegionToWorldSpace(localRegion);
		}

		public BlockCoord ChunkToWorldSpace(ChunkData chunk)
		{
			return this + chunk.WorldSpaceCoord.BlockCoord;
		}

		public int[] ToIntArray()
		{
			return new int[] { x, y, z };
		}

		public static float Distance(BlockCoord a, BlockCoord b)
		{
			int dx = b.x - a.x;
			int dy = b.y - a.y;
			int dz = b.z - a.z;
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static float Distance2D(BlockCoord a, BlockCoord b)
		{
			int dx = b.x - a.x;
			int dz = b.z - a.z;
			return (float)Math.Sqrt(dx * dx + dz * dz);
		}

		public object ToNBT(GameVersion version)
		{
			return new int[] { x, y, z };
		}

		public void FromNBT(object nbtData)
		{
			if(nbtData is int[] data)
			{
				x = data[0];
				y = data[1];
				z = data[2];
			}
			else
			{
				throw new ArgumentException("Invalid NBT data for BlockCoord: " + nbtData);
			}
		}
	}
}

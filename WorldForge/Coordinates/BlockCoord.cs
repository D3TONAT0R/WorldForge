using System;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.Coordinates
{
	public struct BlockCoord : INBTConverter, IEquatable<BlockCoord>
	{
		public static readonly BlockCoord Zero = new BlockCoord(0, 0, 0);

		public int x;
		public int y;
		public int z;

		public RegionLocation Region => new RegionLocation(MathUtils.FastDivFloor(x, 512), MathUtils.FastDivFloor(z, 512));

		public ChunkCoord Chunk => new ChunkCoord(MathUtils.FastDivFloor(x, 16), MathUtils.FastDivFloor(z, 16));

		public BlockCoord LocalRegionCoords => new BlockCoord(x & 511, y, z & 511);

		public BlockCoord LocalChunkCoords => new BlockCoord(x & 15, y, z & 15);

		public BlockCoord LocalSectionCoords => new BlockCoord(x & 15, y & 15, z & 15);

		public BlockCoord Below => new BlockCoord(x, y - 1, z);
		public BlockCoord Above => new BlockCoord(x, y + 1, z);
		public BlockCoord West => new BlockCoord(x - 1, y, z);
		public BlockCoord East => new BlockCoord(x + 1, y, z);
		public BlockCoord North => new BlockCoord(x, y, z - 1);
		public BlockCoord South => new BlockCoord(x, y, z + 1);

		public BlockCoord2D XZ => new BlockCoord2D(x, z);

		public BlockCoord(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public BlockCoord(int[] pos)
		{
			x = pos[0];
			y = pos[1];
			z = pos[2];
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

		public static bool operator ==(BlockCoord l, BlockCoord r)
		{
			return l.x == r.x && l.y == r.y && l.z == r.z;
		}

		public static bool operator !=(BlockCoord l, BlockCoord r)
		{
			return l.x != r.x || l.y != r.y || l.z != r.z;
		}

		public BlockCoord Shift(int dx, int dy, int dz)
		{
			return new BlockCoord(x + dx, y + dy, z + dz);
		}

		public BlockCoord Shift(BlockCoord offset)
		{
			return new BlockCoord(x + offset.x, y + offset.y, z + offset.z);
		}

		public BlockCoord ShiftVertical(int dy)
		{
			return new BlockCoord(x, y + dy, z);
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

		public BlockCoord ChunkToWorldSpace(Chunk chunk)
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
			else if(nbtData is NBTCompound comp)
			{
				comp.TryGet("X", out x);
				comp.TryGet("Y", out y);
				comp.TryGet("Z", out z);
			}
			else
			{
				throw new ArgumentException("Invalid NBT data for BlockCoord: " + nbtData);
			}
		}

		public bool Equals(BlockCoord other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		public override bool Equals(object obj)
		{
			return obj is BlockCoord other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + x.GetHashCode();
				hash = hash * 23 + y.GetHashCode();
				hash = hash * 23 + z.GetHashCode();
				return hash;
			}
		}
	}
}

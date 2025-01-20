using System;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.Coordinates
{
	public struct BlockCoord2D : INBTConverter, IEquatable<BlockCoord2D>
	{
		public static readonly BlockCoord2D Zero = new BlockCoord2D(0, 0);

		public int x;
		public int z;

		public RegionLocation Region => new RegionLocation(MathUtils.FastDivFloor(x, 512), MathUtils.FastDivFloor(z, 512));

		public ChunkCoord Chunk => new ChunkCoord(MathUtils.FastDivFloor(x, 16), MathUtils.FastDivFloor(z, 16));

		public BlockCoord2D LocalRegionCoords => new BlockCoord2D(x & 511, z & 511);

		public BlockCoord2D LocalChunkCoords => new BlockCoord2D(x & 15, z & 15);

		public BlockCoord2D West => new BlockCoord2D(x - 1, z);
		public BlockCoord2D East => new BlockCoord2D(x + 1, z);
		public BlockCoord2D North => new BlockCoord2D(x, z - 1);
		public BlockCoord2D South => new BlockCoord2D(x, z + 1);

		public BlockCoord2D(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public override string ToString()
		{
			return $"[{x},{z}]";
		}

		public static BlockCoord2D operator +(BlockCoord2D l, BlockCoord2D r)
		{
			return new BlockCoord2D(l.x + r.x, l.z + r.z);
		}

		public static BlockCoord2D operator -(BlockCoord2D l, BlockCoord2D r)
		{
			return new BlockCoord2D(l.x - r.x, l.z - r.z);
		}

		public static implicit operator BlockCoord2D((int, int) tuple)
		{
			return new BlockCoord2D(tuple.Item1, tuple.Item2);
		}

		public static implicit operator BlockCoord2D(BlockCoord coord)
		{
			return new BlockCoord2D(coord.x, coord.z);
		}

		public BlockCoord2D Shift(BlockCoord2D offset)
		{
			return new BlockCoord2D(x + offset.x, z + offset.z);
		}

		public int[] ToIntArray()
		{
			return new int[] { x, z };
		}

		public static float Distance(BlockCoord2D a, BlockCoord2D b)
		{
			int dx = b.x - a.x;
			int dz = b.z - a.z;
			return (float)Math.Sqrt(dx * dx + dz * dz);
		}

		public BlockCoord To3D(int y)
		{
			return new BlockCoord(x, y, z);
		}

		public object ToNBT(GameVersion version)
		{
			return new int[] { x, z };
		}

		public void FromNBT(object nbtData)
		{
			if(nbtData is int[] data)
			{
				x = data[0];
				z = data[1];
			}
			else if(nbtData is NBTCompound comp)
			{
				comp.TryGet("X", out x);
				comp.TryGet("Z", out z);
			}
			else
			{
				throw new ArgumentException("Invalid NBT data for BlockCoord2D: " + nbtData);
			}
		}

		public bool Equals(BlockCoord2D other)
		{
			return x == other.x && z == other.z;
		}

		public override bool Equals(object obj)
		{
			return obj is BlockCoord2D other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + x.GetHashCode();
				return hash;
			}
		}

		public static BlockCoord2D Parse(string value)
		{
			var split = value.Split(',');
			int x = int.Parse(split[0].Trim());
			int z = int.Parse(split[1].Trim());
			return new BlockCoord2D(x, z);
		}
	}
}

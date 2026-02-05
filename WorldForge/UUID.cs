using System;
using WorldForge.NBT;

namespace WorldForge
{
	public class UUID : INBTConverter
	{
		public int i0;
		public int i1;
		public int i2;
		public int i3;

		private UUID()
		{
			// Private constructor for internal use
		}

		public UUID(int i0, int i1, int i2, int i3)
		{
			this.i0 = i0;
			this.i1 = i1;
			this.i2 = i2;
			this.i3 = i3;
		}

		public UUID(int[] ints)
		{
			i0 = ints[0];
			i1 = ints[1];
			i2 = ints[2];
			i3 = ints[3];
		}

		public UUID(long mostSignificant, long leastSignificant)
		{
			i0 = (int)(mostSignificant >> 32);
			i1 = (int)mostSignificant;
			i2 = (int)(leastSignificant >> 32);
			i3 = (int)leastSignificant;
		}

		public UUID(string uuidString) 		{
			var hex = uuidString.Replace("-", "");
			if (hex.Length != 32)
			{
				throw new ArgumentException("Invalid UUID string");
			}
			i0 = Convert.ToInt32(hex.Substring(0, 8), 16);
			i1 = Convert.ToInt32(hex.Substring(8, 8), 16);
			i2 = Convert.ToInt32(hex.Substring(16, 8), 16);
			i3 = Convert.ToInt32(hex.Substring(24, 8), 16);
		}

		public static UUID CreateFromNBT(object nbtData)
		{
			UUID uuid = new UUID();
			uuid.FromNBT(nbtData);
			return uuid;
		}

		public object ToNBT(GameVersion version)
		{
			if(version >= GameVersion.Release_1(16))
			{
				return new int[] { i0, i1, i2, i3 };
			}
			else
			{
				return new NBTCompound
				{
					{ "UUIDLeast", ((long)i2 << 32) | i3 },
					{ "UUIDMost", ((long)i0 << 32) | i1 }
				};
			}
		}

		public void FromNBT(object nbtData)
		{
			if(nbtData is int[] ints)
			{
				if(ints.Length != 4)
				{
					throw new ArgumentException("UUID must have 4 integers");
				}
				i0 = ints[0];
				i1 = ints[1];
				i2 = ints[2];
				i3 = ints[3];
			}
			else if(nbtData is long[] longs)
			{
				if(longs.Length != 2)
				{
					throw new ArgumentException("UUID must have 2 longs");
				}
				i0 = (int)(longs[0] >> 32);
				i1 = (int)longs[0];
				i2 = (int)(longs[1] >> 32);
				i3 = (int)longs[1];
			}
			else if(nbtData is NBTCompound comp)
			{
				long least = comp.Get<long>("UUIDLeast");
				long most = comp.Get<long>("UUIDMost");
				i0 = (int)(most >> 32);
				i1 = (int)most;
				i2 = (int)(least >> 32);
				i3 = (int)least;
			}
		}

		public override string ToString() => ToString(true);

		public string ToString(bool dashes)
		{
			if(dashes)
			{
				return $"{i0:x8}-{i1:x8}-{i2:x8}-{i3:x8}";
			}
			else
			{
				return $"{i0:x8}{i1:x8}{i2:x8}{i3:x8}";
			}
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + i0.GetHashCode();
			hash = hash * 31 + i1.GetHashCode();
			hash = hash * 31 + i2.GetHashCode();
			hash = hash * 31 + i3.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			if(obj is UUID other)
			{
				return i0 == other.i0 && i1 == other.i1 && i2 == other.i2 && i3 == other.i3;
			}
			return false;
		}
	}
}

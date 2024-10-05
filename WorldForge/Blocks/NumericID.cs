using System;

namespace WorldForge
{
	public struct NumericID : IEquatable<NumericID>
	{
		public static readonly NumericID Air = new NumericID(0);

		public short id;
		public short damage;

		public NumericID WithoutDamage => new NumericID(id, 0);

		public NumericID(short id, short damage = 0)
		{
			this.id = id;
			this.damage = damage;
		}

		public static NumericID? TryParse(string s)
		{
			try
			{
				if(string.IsNullOrWhiteSpace(s)) return null;
				var split = s.Split(':');
				short id = short.Parse(split[0]);
				short dmg = 0;
				if(split.Length > 1) short.TryParse(split[1], out dmg);
				return new NumericID(id, dmg);
			}
			catch
			{
				return null;
			}
		}

		public uint Hash => (uint)((id << 16) + damage);

		public uint HashNoDamage => (uint)(id << 16);

		public override string ToString()
		{
			return id + ":" + damage;
		}

		public bool Equals(NumericID other)
		{
			return id == other.id && damage == other.damage;
		}

		public override bool Equals(object obj)
		{
			return obj is NumericID other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (id.GetHashCode() * 397) ^ damage.GetHashCode();
			}
		}
	}
}

namespace WorldForge
{
	public struct NumericID
	{
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
	}
}

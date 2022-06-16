using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils
{
	public struct NumericID
	{
		public byte id;
		public byte meta;

		public NumericID(byte id, byte meta = 0)
		{
			this.id = id;
			this.meta = meta;
		}

		public static NumericID? TryParse(string s)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(s)) return null;
				var split = s.Split(':');
				byte id = byte.Parse(split[0]);
				byte meta = 0;
				if (split.Length > 1) byte.TryParse(split[1], out meta);
				return new NumericID(id, meta);
			}
			catch
			{
				return null;
			}
		}

		public ushort Hash => (ushort)((id << 8) + meta);

		public override string ToString()
		{
			return id + ":" + meta;
		}
	}
}

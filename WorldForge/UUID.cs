using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class UUID : INBTConverter
	{
		public int i0;
		public int i1;
		public int i2;
		public int i3;

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

		private UUID()
		{
			//Needed for Activator.CreateInstance
		}

		public object ToNBT(GameVersion version)
		{
			//TODO: version differences
			return new int[] { i0, i1, i2, i3 };
		}

		public void FromNBT(object nbtData)
		{
			var ints = (int[])nbtData;
			if(ints.Length != 4)
			{
				throw new ArgumentException("UUID must have 4 integers");
			}
			i0 = ints[0];
			i1 = ints[1];
			i2 = ints[2];
			i3 = ints[3];
		}
	}
}

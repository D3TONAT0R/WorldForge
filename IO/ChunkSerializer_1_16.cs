using System;
using System.Collections.Generic;
using System.Text;
using MCUtils;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class ChunkSerializer_1_16 : ChunkSerializer_1_13
	{
		public ChunkSerializer_1_16(Version version) : base(version) { }

		protected override string AppendBlockStateBitsToBitStream(string bitStream, string newBits, int indexBitCount)
		{
			bitStream += newBits.Substring(0, (int)Math.Floor(newBits.Length / (double)indexBitCount) * indexBitCount);
			return bitStream;
		}
	}
}

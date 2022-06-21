using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.IO
{
	public class ChunkSerializer_1_19 : ChunkSerializer_1_16
	{
		public ChunkSerializer_1_19(Version version) : base(version) { }

		protected override bool HasBlocks(NBTContent.CompoundContainer sectionNBT)
		{
			return sectionNBT.Contains("block_states");
		}

		protected override NBTContent.ListContainer GetBlockPalette(NBTContent.CompoundContainer sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").GetAsList("palette");
		}

		protected override long[] GetBlockDataArray(NBTContent.CompoundContainer sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").Get<long[]>("data");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class BlockState
	{

		public static readonly BlockState air = new BlockState(ProtoBlock.RegisterNewVanillaBlock("air", Version.FirstVersion));

		public ProtoBlock block;
		public CompoundContainer properties = new CompoundContainer();

		public BlockState(ProtoBlock blockType)
		{
			block = blockType;
			AddDefaultBlockProperties();
		}

		void AddDefaultBlockProperties()
		{
			if (block == null) return;
			switch (block.shortID)
			{
				case "oak_leaves":
				case "spruce_leaves":
				case "birch_leaves":
				case "jungle_leaves":
				case "acacia_leaves":
				case "dark_oak_leaves":
					//TODO: proper distance
					properties.Add("distance", 1);
					break;
			}
		}

		public bool Compare(BlockState other, bool compareProperties = true)
		{
			if (compareProperties)
			{
				if (!CompoundContainer.AreEqual(properties, other.properties)) return false;
			}
			return block == other.block;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(block?.ID);
			if(properties.cont.Count > 0)
			{
				sb.Append("[");
				bool first = true;
				foreach(var prop in properties.cont)
				{
					if (!first) sb.Append(",");
					first = false;
					sb.Append($"{prop.Key}={prop.Value}");
				}
				sb.Append("]");
			}
			return sb.ToString();
		}
	}
}

using MCUtils.NBT;
using System;
using System.Text;

namespace MCUtils
{
	public class BlockState
	{

		public static BlockState Air
		{
			get
			{
				if (air == null) air = new BlockState(BlockList.Find("air"));
				return air;
			}
		}
		private static BlockState air;

		public static BlockState Unknown
		{
			get
			{
				if (unknown == null) unknown = new BlockState();
				return unknown;
			}
		}
		private static BlockState unknown;

		public ProtoBlock block;
		public NBTCompound properties = new NBTCompound();

		private BlockState() { }

		public BlockState(ProtoBlock blockType)
		{
			if(blockType == null)
			{
				throw new NullReferenceException("Attempted to create a BlockState with a null ProtoBlock.");
			}
			block = blockType;
			AddDefaultBlockProperties();
		}

		public BlockState(string blockType) : this(BlockList.Find(blockType))
		{

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
				if (!NBTCompound.AreEqual(properties, other.properties)) return false;
			}
			return block == other.block;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(block?.ID);
			if(properties.contents.Count > 0)
			{
				sb.Append("[");
				bool first = true;
				foreach(var prop in properties.contents)
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

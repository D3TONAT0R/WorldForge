using MCUtils.NBT;
using System;
using System.Text;

namespace MCUtils
{
	public class BlockState
	{

		public struct Property
		{
			public string id;
			public string state;

			public Property(string id, string state)
			{
				this.id = id;
				this.state = state;
			}

			public static implicit operator Property((string, string) tuple)
			{
				return new Property(tuple.Item1, tuple.Item2);
			}
		}

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
		//TODO: Don't auto-generate an empty compound, generates excessive amounts of garbage.
		//Add methods for SetProperty / GetProperty / RemoveProperty ... instead.
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

		public BlockState(string blockType, params Property[] properties) : this(BlockList.Find(blockType))
		{
			foreach(var prop in properties)
			{
				this.properties.Set(prop.id, prop.state);
			}
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

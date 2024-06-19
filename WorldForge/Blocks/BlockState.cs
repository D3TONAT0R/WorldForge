using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class BlockState
	{

		public static BlockState Air
		{
			get
			{
				if(air == null) air = new BlockState(BlockList.Find("air"));
				return air;
			}
		}
		private static BlockState air;

		public static BlockState Unknown
		{
			get
			{
				if(unknown == null) unknown = new BlockState();
				return unknown;
			}
		}
		private static BlockState unknown;

		public ProtoBlock block;
		private NBTCompound properties;

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

		public BlockState(string blockType, params (string, string)[] properties) : this(BlockList.Find(blockType))
		{
			foreach(var prop in properties)
			{
				this.properties.Set(prop.Item1, prop.Item2);
			}
		}

		public BlockState(NBTCompound paletteNBT) : this(BlockList.Find(paletteNBT.Get<string>("Name")))
		{
			paletteNBT.TryGet("Properties", out properties);
		}

		public bool HasProperty(string key)
		{
			if(properties == null) return false;
			return properties.Contains(key);
		}

		public string GetProperty(string key)
		{
			if(properties == null) return null;
			if(properties.TryGet(key, out string v))
			{
				return v;
			}
			else return null;
		}

		public bool TryGetProperty(string key, out string value)
		{
			value = GetProperty(key);
			return value != null;
		}

		public void SetProperty(string key, string value)
		{
			if(properties == null) properties = new NBTCompound();
			properties.Set(key, value);
		}

		public bool RemoveProperty(string key)
		{
			if(properties == null) return false;
			return properties.Remove(key);
		}

		void AddDefaultBlockProperties()
		{
			if(block == null) return;
			switch(block.shortID)
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
			if(compareProperties)
			{
				if(!NBTCompound.AreEqual(properties, other.properties)) return false;
			}
			return block == other.block;
		}

		public NBTCompound ToPaletteNBT()
		{
			var nbt = new NBTCompound();
			nbt.Add("Name", block.ID);
			if(!NBTCompound.IsNullOrEmpty(properties))
			{
				nbt.Add("Properties", properties);
			}
			return nbt;
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
					if(!first) sb.Append(",");
					first = false;
					sb.Append($"{prop.Key}={prop.Value}");
				}
				sb.Append("]");
			}
			return sb.ToString();
		}
	}
}

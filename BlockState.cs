using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class BlockState
	{

		public static readonly BlockState air = new BlockState("minecraft:air");

		public string ID => (customNamespace ?? "minecraft") + ":" + shortID;
		public readonly string customNamespace = null;
		public readonly string shortID;
		public CompoundContainer properties = new CompoundContainer();

		public BlockState(string name)
		{
			if (!name.Contains(":"))
			{
				name = "minecraft:" + name;
			}
			var split = name.Split(':');
			customNamespace = split[0] == "minecraft" ? null : split[0];
			name = split[1];
			shortID = name;
			AddDefaultBlockProperties();
		}

		void AddDefaultBlockProperties()
		{
			switch (shortID)
			{
				case "oak_leaves":
				case "spruce_leaves":
				case "birch_leaves":
				case "jungle_leaves":
				case "acacia_leaves":
				case "dark_oak_leaves":
					properties.Add("distance", 1);
					break;
			}
		}

		public bool CompareMultiple(params string[] ids)
		{
			bool b = false;
			for (int i = 0; i < ids.Length; i++)
			{
				b |= Compare(ids[i]);
			}
			return b;
		}

		public bool Compare(string block)
		{
			return block == ID;
		}

		public bool Compare(BlockState state, bool compareProperties)
		{
			if (compareProperties)
			{
				if (!CompoundContainer.AreEqual(properties, state.properties)) return false;
			}
			return state.ID == ID;
		}
	}
}

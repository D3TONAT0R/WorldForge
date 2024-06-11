using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct Dimension : INBTConverter
	{
		public string ID { get; private set; }

		public static readonly Dimension Overworld = new Dimension("minecraft:overworld");
		public static readonly Dimension Nether = new Dimension("minecraft:the_nether");
		public static readonly Dimension End = new Dimension("minecraft:the_end");

		public int? DimensionIndex
		{
			get
			{
				switch(ID)
				{
					case "minecraft:overworld": return 0;
					case "minecraft:the_nether": return -1;
					case "minecraft:the_end": return 1;
					default: return null;
				}
			}
		}

		public Dimension(string id)
		{
			ID = id;
		}

		public static Dimension FromIndex(int index)
		{
			switch(index)
			{
				case 0: return Overworld;
				case -1: return Nether;
				case 1: return End;
				default: throw new ArgumentException(nameof(index), "Unknown dimension index: " + index);
			}
		}

		public object ToNBT(GameVersion version)
		{
			return ID;
		}

		public void FromNBT(object nbtData)
		{
			if(nbtData is string s)
			{
				ID = s;
			}
			else if(nbtData is int i)
			{
				ID = FromIndex(i).ID;
			}
			else if(nbtData is byte b)
			{
				ID = FromIndex(b).ID;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct DimensionID : INBTConverter
	{
		public string ID { get; private set; }

		public static readonly DimensionID Overworld = new DimensionID("minecraft:overworld");
		public static readonly DimensionID Nether = new DimensionID("minecraft:the_nether");
		public static readonly DimensionID End = new DimensionID("minecraft:the_end");

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

		public DimensionID(string id)
		{
			ID = id;
		}

		public static DimensionID FromIndex(int index)
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

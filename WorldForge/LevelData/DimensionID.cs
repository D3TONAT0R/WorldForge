using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct DimensionID : INBTConverter, IEquatable<DimensionID>
	{

		public string ID { get; private set; }

		public static readonly DimensionID Unknown = new DimensionID(null);
		public static readonly DimensionID Overworld = new DimensionID("minecraft:overworld");
		public static readonly DimensionID Nether = new DimensionID("minecraft:the_nether");
		public static readonly DimensionID TheEnd = new DimensionID("minecraft:the_end");

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

		public bool Exists => ID != null;

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
				case 1: return TheEnd;
				default: return Unknown;
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

		public bool Equals(DimensionID other)
		{
			return ID == other.ID;
		}

		public override bool Equals(object obj)
		{
			return obj is DimensionID other && Equals(other);
		}

		public override int GetHashCode()
		{
			return ID != null ? ID.GetHashCode() : 0;
		}

		public override string ToString()
		{
			return ID;
		}

		public static bool operator ==(DimensionID left, DimensionID right)
		{
			return left.ID == right.ID;
		}

		public static bool operator !=(DimensionID left, DimensionID right)
		{
			return left.ID != right.ID;
		}
	}
}

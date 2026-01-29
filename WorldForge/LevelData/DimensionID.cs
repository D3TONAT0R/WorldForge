using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct DimensionID : INBTConverter, IEquatable<DimensionID>
	{

		public string ID { get; private set; }

		public int Index { get; private set; }

		public string SubdirectoryName => (Index != 0 && Index != int.MinValue) ? $"DIM{Index}" : null;

		public static readonly DimensionID Unknown = new DimensionID(null, int.MinValue);
		public static readonly DimensionID Overworld = new DimensionID("minecraft:overworld", 0);
		public static readonly DimensionID Nether = new DimensionID("minecraft:the_nether", -1);
		public static readonly DimensionID TheEnd = new DimensionID("minecraft:the_end", 1);

		private static List<DimensionID> registry = new List<DimensionID>
		{
			Overworld,
			Nether,
			TheEnd
		};

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

		private DimensionID(string id, int index)
		{
			ID = id;
			Index = index;
		}

		public static DimensionID Register(string ID, int index = int.MinValue)
		{
			var b = new DimensionID(ID, index);
			registry.Add(b);
			return b;
		}

		public static DimensionID FromID(string id)
		{
			foreach (var b in registry)
			{
				if (b.ID == id)
				{
					return b;
				}
			}
			return Register(id);
		}

		public static DimensionID FromIndex(int index)
		{
			foreach(var b in registry)
			{
				if(b.Index == index)
				{
					return b;
				}
			}
			return Register("unknown:unknown", index);
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

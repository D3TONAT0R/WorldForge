using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct DimensionID : INBTConverter, IEquatable<DimensionID>
	{

		public NamespacedID ID { get; private set; }

		public int? Index { get; private set; }

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

		public bool Exists => ID != NamespacedID.unknown;

		private DimensionID(string id, int? index)
		{
			ID = new NamespacedID(id ?? "unknown:unknown");
			Index = index;
		}

		public static DimensionID Register(string ID, int? index = null)
		{
			var b = new DimensionID(ID, index);
			registry.Add(b);
			return b;
		}

		public static DimensionID Temporary(string ID, int? index = null)
		{
			return new DimensionID(ID, index);
		}

		public static DimensionID FromID(string id)
		{
			var namespacedID = new NamespacedID(id);
			foreach (var b in registry)
			{
				if (b.ID == namespacedID)
				{
					return b;
				}
			}
			return Register(id);
		}

		public static DimensionID FromIndex(int index)
		{
			foreach (var b in registry)
			{
				if (b.Index == index)
				{
					return b;
				}
			}
			return Register("unknown:unknown", index);
		}

		public string GetSubdirectoryName(bool forceDimensionsDirectory)
		{
			if (Index == 0 && !forceDimensionsDirectory) return "";
			if (!forceDimensionsDirectory)
			{
				if (Index != null) return "DIM" + Index.Value;
				else return Path.Combine("dimensions", ID.ResolvedNamespace, ID.id);
			}
			else
			{
				if (ID != NamespacedID.unknown) return Path.Combine("dimensions", ID.ResolvedNamespace, ID.id);
				else return Path.Combine("dimensions", "unknown", "DIM" + Index.Value);
			}
		}

		public object ToNBT(GameVersion version)
		{
			return ID;
		}

		public void FromNBT(object nbtData)
		{
			if (nbtData is string s)
			{
				ID = new NamespacedID(s);
			}
			else if (nbtData is int i)
			{
				ID = FromIndex(i).ID;
			}
			else if (nbtData is byte b)
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
			if (ID != null)
			{
				int h = ID.GetHashCode();
				return h;
			}
			return 0;
			//return ID != null ? ID.GetHashCode() : 0;
		}

		public override string ToString()
		{
			return ID.FullID;
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

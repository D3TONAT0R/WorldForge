using System;
using System.Linq;
using WorldForge.NBT;

namespace WorldForge
{
	public readonly struct NamespacedID : IEquatable<NamespacedID>, INBTConverter
	{
		public readonly string customNamespace;
		public readonly string id;

		public readonly int hash;

		public string FullID => (customNamespace ?? "minecraft") + ":" + id;

		public bool HasCustomNamespace => customNamespace != null;

		public NamespacedID(string ns, string id, bool checkValidity = true)
		{
			if(checkValidity)
			{
				if(ns != null && !CheckValidity(ns, true, out char c)) throw new System.ArgumentException($"Illegal character '{c}' in namespace '{ns}'.");
				if(!CheckValidity(id, true, out c)) throw new System.ArgumentException($"Illegal character '{c}' in ID '{id}'.");
			}
			customNamespace = ns;
			this.id = id;
			hash = 0;
			hash = FullID.GetHashCode();
		}

		public NamespacedID(string fullId, bool checkValidity = true)
		{
			if(checkValidity && !CheckValidity(fullId, false, out char c)) throw new System.ArgumentException($"Illegal character '{c}' in id '{fullId}'.");
			var split = fullId.Split(':');
			if(split.Length == 1)
			{
				customNamespace = null;
				id = split[0];
			}
			else
			{
				customNamespace = split[0] != "minecraft" ? split[0] : null;
				id = split[1];
			}
			hash = 0;
			hash = FullID.GetHashCode();
		}

		public override string ToString()
		{
			return FullID;
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public static bool operator ==(NamespacedID l, NamespacedID r)
		{
			return l.FullID == r.FullID;
		}

		public static bool operator !=(NamespacedID l, NamespacedID r)
		{
			return !(l == r);
		}

		public bool Matches(string id)
		{
			var split = id.Split(':');
			if(split.Length == 1)
			{
				return id == this.id;
			}
			else
			{
				if(split[0] == "minecraft") split[0] = null;
				return customNamespace == split[0] && this.id == split[1];
			}
		}

		public bool Equals(NamespacedID other)
		{
			return customNamespace == other.customNamespace && id == other.id;
		}

		public override bool Equals(object obj)
		{
			return obj is NamespacedID other && Equals(other);
		}

		public static implicit operator NamespacedID(string id)
		{
			return new NamespacedID(id);
		}

		public static bool CheckValidity(string id, bool forceNoNamespace, out char illegalChar)
		{
			if(forceNoNamespace && id.Contains(":"))
			{
				illegalChar = ':';
				return false;
			}
			if(CountOccurrences(id, ':') > 1)
			{
				illegalChar = ':';
				return false;
			}

			for(var i = 0; i < id.Length; i++)
			{
				var c = id[i];
				if(!IsValidCharacter(c))
				{
					illegalChar = c;
					return false;
				}
			}

			illegalChar = '\0';
			return true;
		}

		private static bool IsValidCharacter(char c)
		{
			if(char.IsLetter(c)) return char.IsLower(c);
			return char.IsDigit(c) || c == '_' || c == ':';
		}

		private static int CountOccurrences(string str, char c)
		{
			int count = 0;
			for(var i = 0; i < str.Length; i++)
			{
				var ch = str[i];
				if(ch == c) count++;
			}
			return count;
		}

		public object ToNBT(GameVersion version)
		{
			return FullID;
		}

		public void FromNBT(object nbtData)
		{
			throw new NotSupportedException();
		}
	}
}

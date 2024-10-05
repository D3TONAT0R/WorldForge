using System;
using System.Linq;
using WorldForge.NBT;

namespace WorldForge
{
	public struct NamespacedID : IEquatable<NamespacedID>, INBTConverter
	{
		public string customNamespace;
		public string id;

		public string FullID => (customNamespace ?? "minecraft") + ":" + id;

		public bool HasCustomNamespace => customNamespace != null;

		public NamespacedID(string ns, string id)
		{
			if(ns != null && !CheckValidity(ns, true, out char c)) throw new System.ArgumentException($"Illegal character '{c}' in namespace '{ns}'.");
			if(!CheckValidity(id, true, out c)) throw new System.ArgumentException($"Illegal character '{c}' in ID '{id}'.");
			customNamespace = ns;
			this.id = id;
		}

		public NamespacedID(string fullId)
		{
			if(!CheckValidity(fullId, false, out char c)) throw new System.ArgumentException($"Illegal character '{c}' in id '{fullId}'.");
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
		}

		private void Set(string fullId)
		{

		}

		public override string ToString()
		{
			return FullID;
		}

		public override int GetHashCode()
		{
			return FullID.GetHashCode();
		}

		public static bool operator ==(NamespacedID l, NamespacedID r)
		{
			return l.FullID == r.FullID;
		}

		public static bool operator !=(NamespacedID l, NamespacedID r)
		{
			return !(l == r);
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
			const string allowedChars = "abcdefghijklmnopqrstuvwxyz0123456789_:";
			if(forceNoNamespace && id.Contains(":"))
			{
				illegalChar = ':';
				return false;
			}
			if(id.Count(c => c == ':') > 1)
			{
				illegalChar = ':';
				return false;
			}
			foreach(char c in id)
			{
				if(!allowedChars.Contains(c))
				{
					illegalChar = c;
					return false;
				}
			}
			illegalChar = '\0';
			return true;
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

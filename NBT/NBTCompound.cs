using System;
using System.Collections;
using System.Collections.Generic;

namespace MCUtils.NBT
{
	///<summary>A container for the TAG_Compound tag.</summary>
	public class NBTCompound : AbstractNBTContainer, IEnumerable<KeyValuePair<string, object>>
	{

		public override NBTTag containerType
		{
			get { return NBTTag.TAG_Compound; }
		}

		public Dictionary<string, object> cont = new Dictionary<string, object>();

		public NBTCompound() : base()
		{
			
		}

		public NBTCompound(IDictionary<string, object> contents) : base()
		{
			foreach(var k in contents.Keys)
			{
				cont.Add(k, contents[k]);
			}
		}

		public T Add<T>(string key, T value)
		{
			if (value is bool b)
			{
				Add(key, (byte)(b ? 1 : 0));
				return value;
			}
			else if(value is INBTCompatible i)
			{
				Add(key, i.GetNBTCompatibleObject());
				return value;
			}
			cont.Add(key, value);
			return value;
		}

		public void AddRange(params (string key, object obj)[] values)
		{
			foreach (var (key, obj) in values)
			{
				Add(key, obj);
			}
		}

		public NBTCompound AddCompound(string key)
		{
			return Add(key, new NBTCompound());
		}

		public NBTList AddList(string key, NBTTag tag)
		{
			return Add(key, new NBTList(tag));
		}

		public T Set<T>(string key, T value)
		{
			if (Contains(key))
			{
				cont.Remove(key);
			}
			Add(key, value);
			return value;
		}

		public object Get(string key)
		{
			if (cont.ContainsKey(key))
			{
				return cont[key];
			}
			else
			{
				MCUtilsConsole.WriteError("Key '" + key + "' does not exist!");
				return null;
			}
		}

		public T Get<T>(string key)
		{
			return (T)Convert.ChangeType(Get(key), typeof(T));
		}

		public bool TryGet<T>(string key, out T value)
		{
			if (Contains(key))
			{
				try
				{
					value = (T)Convert.ChangeType(Get(key), typeof(T));
					return true;
				}
				catch
				{
					value = default;
					return false;
				}
			}
			else
			{
				value = default;
				return false;
			}
		}

		public bool Remove(string key)
		{
			return cont.Remove(key);
		}

		public bool Contains(string key)
		{
			return cont.ContainsKey(key);
		}

		public NBTCompound GetAsCompound(string key)
		{
			return Get(key) as NBTCompound;
		}

		public NBTList GetAsList(string key)
		{
			return Get(key) as NBTList;
		}

		public static bool AreEqual(NBTCompound a, NBTCompound b)
		{
			if (a == null && b == null) return true;
			if ((a == null) != (b == null)) return false;
			return a.HasSameContent(b);
		}

		public bool HasSameContent(NBTCompound other)
		{
			if (other.cont.Keys.Count != cont.Keys.Count) return false;
			foreach (string k in cont.Keys)
			{
				if (!other.Contains(k)) return false;
				if (!cont[k].Equals(other.cont[k])) return false;
			}
			return true;
		}

		public override string[] GetContentKeys(string prefix = null)
		{
			string[] k = new string[cont.Count];
			int i = 0;
			foreach (var key in cont.Keys)
			{
				k[i] = prefix + key;
				i++;
			}
			return k;
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return cont.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return cont.GetEnumerator();
		}
	}
}

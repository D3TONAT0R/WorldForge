using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldForge.NBT
{
	///<summary>A container for the TAG_Compound tag.</summary>
	public class NBTCompound : INBTContainer, IEnumerable<KeyValuePair<string, object>>
	{
		public NBTTag ContainerType => NBTTag.TAG_Compound;

		public int ItemCount => contents.Count;

		public Dictionary<string, object> contents = new Dictionary<string, object>();

		public static bool IsNullOrEmpty(NBTCompound c)
		{
			return c == null || c.ItemCount == 0;
		}

		public NBTCompound() : base()
		{

		}

		public NBTCompound(IDictionary<string, object> contents) : base()
		{
			foreach(var k in contents.Keys)
			{
				this.contents.Add(k, contents[k]);
			}
		}

		public static NBTCompound FromObject(object obj, GameVersion? version = null)
		{
			var comp = new NBTCompound();
			NBTConverter.WriteToNBT(obj, comp, version ?? GameVersion.FirstVersion);
			return comp;
		}

		public T Add<T>(string key, T value)
		{
			if(key == null)
			{
				throw new NullReferenceException("Attempted to add a null key.");
			}
			if(value == null)
			{
				throw new NullReferenceException("Attempted to add null value: " + key);
			}
			if(value is bool b)
			{
				Add(key, (byte)(b ? 1 : 0));
				return value;
			}
			else if(value is INBTConverter i)
			{
				Add(key, i.ToNBT(GameVersion.FirstVersion));
				return value;
			}
			contents.Add(key, value);
			return value;
		}

		public void AddRange(params (string key, object obj)[] values)
		{
			foreach(var (key, obj) in values)
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
			if(value == null)
			{
				throw new NullReferenceException("Attempted to set null value: " + key);
			}
			if(Contains(key))
			{
				contents.Remove(key);
			}
			Add(key, value);
			return value;
		}

		public object Get(string key)
		{
			if(contents.ContainsKey(key))
			{
				return contents[key];
			}
			else
			{
				Logger.Error("Key '" + key + "' does not exist!");
				return null;
			}
		}

		public T Get<T>(string key)
		{
			var v = Get(key);
			if(v is NBTList list && typeof(T) != typeof(NBTList))
			{
				//Check if T is List<> or array
				bool isList = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>);
				if(isList)
				{
					return (T)list.ToList(typeof(T).GetGenericArguments()[0]);
				}
				else if(typeof(T).IsArray)
				{
					var array = Array.CreateInstance(typeof(T).GetElementType(), list.Length);
					var ilist = list.ToList(typeof(T).GetElementType());
					ilist.CopyTo(array, 0);
					return (T)Convert.ChangeType(array, typeof(T));
				}
				else
				{
					throw new InvalidOperationException("Invalid type given: "+typeof(T));
				}
			}
			else
			{
				return (T)Convert.ChangeType(v, typeof(T));
			}
		}

		public bool TryGet<T>(string key, out T value)
		{
			if(Contains(key))
			{
				try
				{
					value = Get<T>(key);
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
			return contents.Remove(key);
		}

		public object Take(string key)
		{
			object value = Get(key);
			contents.Remove(key);
			return value;
		}

		public T Take<T>(string key)
		{
			T value = Get<T>(key);
			contents.Remove(key);
			return value;
		}

		public bool TryTake<T>(string key, out T value)
		{
			if(TryGet(key, out value))
			{
				contents.Remove(key);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Contains(string key)
		{
			return contents.ContainsKey(key);
		}

		public bool CheckType<T>(string key)
		{
			return Get(key) is T;
		}

		public NBTCompound GetAsCompound(string key)
		{
			return Get(key) as NBTCompound;
		}

		public NBTList GetAsList(string key)
		{
			return Get(key) as NBTList;
		}

		public string FindKey(object value)
		{
			foreach(var kv in contents)
			{
				if(kv.Value.Equals(value)) return kv.Key;
			}
			return null;
		}

		public void PackAll(string key)
		{
			NBTCompound group = new NBTCompound();
			group.contents = contents;
			contents = new Dictionary<string, object>();
			contents.Add(key, group);
		}

		public void UnpackInto(NBTCompound parent)
		{
			foreach(var child in this)
			{
				parent.Add(child.Key, child.Value);
			}
			var k = parent.FindKey(this);
			parent.Remove(k);
		}

		public void Merge(NBTCompound target, bool overwrite)
		{
			foreach(var kv in this)
			{
				if(target.Contains(kv.Key) && !overwrite)
				{
					continue;
				}
				target.Set(kv.Key, kv.Value);
			}
		}

		public static bool AreEqual(NBTCompound a, NBTCompound b)
		{
			if(a == null && b == null) return true;
			if((a == null) != (b == null)) return false;
			return a.HasSameContent(b);
		}

		public bool HasSameContent(NBTCompound other)
		{
			if(other.contents.Keys.Count != contents.Keys.Count) return false;
			foreach(string k in contents.Keys)
			{
				if(!other.Contains(k)) return false;
				if(!contents[k].Equals(other.contents[k])) return false;
			}
			return true;
		}

		public string[] GetContentKeys(string prefix = null)
		{
			string[] k = new string[contents.Count];
			int i = 0;
			foreach(var key in contents.Keys)
			{
				k[i] = prefix + key;
				i++;
			}
			return k;
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return contents.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return contents.GetEnumerator();
		}

		public NBTCompound Clone()
		{
			return new NBTCompound(contents);
		}

		public override string ToString()
		{
			return $"{ContainerType}[{ItemCount}]";
		}
	}
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
			foreach (var k in contents.Keys)
			{
				this.contents.Add(k, contents[k]);
			}
		}

		public static NBTCompound FromJson(string jsonString)
		{
			return FromJson(JObject.Parse(jsonString));
		}

		public static NBTCompound FromJson(JObject obj)
		{
			var nbt = new NBTCompound();
			foreach (var prop in obj)
			{
				string key = prop.Key;
				JToken value = prop.Value;
				nbt.Add(key, ParseJValue(value));
			}
			return nbt;
		}

		private static object ParseJValue(JToken value)
		{
			switch (value.Type)
			{
				case JTokenType.Boolean:
					return (bool)value;
				case JTokenType.Integer:
					return (long)value;
				case JTokenType.Float:
					return (double)value;
				case JTokenType.String:
					return (string)value;
				case JTokenType.Object:
					return FromJson(value.Value<JObject>());
				case JTokenType.Array:
					var array = value.Value<JArray>();
					var jType = array.First?.Type ?? JTokenType.None;
					var list = new NBTList(JTokenTypeToNBTTag(jType));
					foreach (var item in array)
					{
						var value1 = ParseJValue((JValue)item);
						list.Add(value1);
					}
					return list;
				case JTokenType.Constructor:
				case JTokenType.Property:
				case JTokenType.Comment:
				case JTokenType.Null:
				case JTokenType.Undefined:
				case JTokenType.Date:
				case JTokenType.Raw:
				case JTokenType.Bytes:
				case JTokenType.Guid:
				case JTokenType.Uri:
				case JTokenType.TimeSpan:
				default:
					throw new NotSupportedException($"Unsupported JTokenType " + value.Type);
			}
		}

		private static NBTTag JTokenTypeToNBTTag(JTokenType type)
		{
			switch (type)
			{
				case JTokenType.None:
					return NBTTag.TAG_End;
				case JTokenType.Boolean:
					return NBTTag.TAG_Byte;
				case JTokenType.Integer:
					return NBTTag.TAG_Long;
				case JTokenType.Float:
					return NBTTag.TAG_Double;
				case JTokenType.String:
					return NBTTag.TAG_String;
				case JTokenType.Object:
					return NBTTag.TAG_Compound;
				case JTokenType.Array:
					return NBTTag.TAG_List;
				case JTokenType.Constructor:
				case JTokenType.Property:
				case JTokenType.Comment:
				case JTokenType.Null:
				case JTokenType.Undefined:
				case JTokenType.Date:
				case JTokenType.Raw:
				case JTokenType.Bytes:
				case JTokenType.Guid:
				case JTokenType.Uri:
				case JTokenType.TimeSpan:
				default:
					throw new NotSupportedException($"Unsupported JTokenType " + type);
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
			if (key == null)
			{
				throw new NullReferenceException("Attempted to add a null key.");
			}
			if (value == null)
			{
				throw new NullReferenceException("Attempted to add null value: " + key);
			}
			if (value is bool b)
			{
				Add(key, (byte)(b ? 1 : 0));
				return value;
			}
			else if (value is INBTConverter i)
			{
				Add(key, i.ToNBT(GameVersion.FirstVersion));
				return value;
			}
			contents.Add(key, value);
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
			if (value == null)
			{
				throw new NullReferenceException("Attempted to set null value: " + key);
			}
			if (Contains(key))
			{
				contents.Remove(key);
			}
			Add(key, value);
			return value;
		}

		public object Get(string key)
		{
			if (contents.ContainsKey(key))
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
			if (v is NBTList list && typeof(T) != typeof(NBTList))
			{
				//Check if T is List<> or array
				bool isList = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>);
				if (isList)
				{
					return (T)list.ToList(typeof(T).GetGenericArguments()[0]);
				}
				else if (typeof(T).IsArray)
				{
					var array = Array.CreateInstance(typeof(T).GetElementType(), list.Length);
					var ilist = list.ToList(typeof(T).GetElementType());
					ilist.CopyTo(array, 0);
					return Cast<T>(array);
				}
				else
				{
					throw new InvalidOperationException("Invalid type given: " + typeof(T));
				}
			}
			else
			{
				return Cast<T>(v);
			}
		}

		private T Cast<T>(object obj)
		{
			if (obj is T t) return t;
			//Special case for byte to sbyte conversion to avoid overflow exceptions
			if (obj is byte b && typeof(T) == typeof(sbyte))
			{
				unchecked
				{
					sbyte sb = (sbyte)b;
					return (T)Convert.ChangeType(sb, typeof(T));
				}
			}
			return (T)Convert.ChangeType(obj, typeof(T));
		}

		public bool TryGet<T>(string key, out T value)
		{
			if (Contains(key))
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
			if (TryGet(key, out value))
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
			foreach (var kv in contents)
			{
				if (kv.Value.Equals(value)) return kv.Key;
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
			foreach (var child in this)
			{
				parent.Add(child.Key, child.Value);
			}
			var k = parent.FindKey(this);
			parent.Remove(k);
		}

		public void Merge(NBTCompound target, bool overwrite)
		{
			foreach (var kv in this)
			{
				if (target.Contains(kv.Key) && !overwrite)
				{
					continue;
				}
				target.Set(kv.Key, kv.Value);
			}
		}

		public static bool AreEqual(NBTCompound a, NBTCompound b)
		{
			if (a == null && b == null) return true;
			if ((a == null) != (b == null)) return false;
			return a.HasSameContent(b);
		}

		public bool HasSameContent(NBTCompound other)
		{
			if (other.contents.Keys.Count != contents.Keys.Count) return false;
			foreach (string k in contents.Keys)
			{
				if (!other.Contains(k)) return false;
				if (!contents[k].Equals(other.contents[k])) return false;
			}
			return true;
		}

		public string[] GetContentKeys(string prefix = null)
		{
			string[] k = new string[contents.Count];
			int i = 0;
			foreach (var key in contents.Keys)
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

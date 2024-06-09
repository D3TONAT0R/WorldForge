using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldForge.NBT
{
	///<summary>A container for the TAG_List tag.</summary>
	public class NBTList : AbstractNBTContainer, IEnumerable<object>
	{

		public override NBTTag containerType => NBTTag.TAG_List;

		public NBTTag contentsType;
		public int Length
		{
			get
			{
				return listContent.Count;
			}
		}
		public List<object> listContent = new List<object>();

		public NBTList(NBTTag baseType) : base()
		{
			contentsType = baseType;
		}

		public NBTList(NBTTag baseType, IEnumerable<object> contents) : this(baseType)
		{
			foreach(var c in contents)
			{
				Add(c);
			}
		}

		public static NBTList FromList<T>(List<T> list)
		{
			var nbtList = new NBTList(NBTMappings.GetTag(typeof(T)));
			foreach(var item in list)
			{
				nbtList.Add(item);
			}
			return nbtList;
		}


		public object Get(int index)
		{
			return this[index];
		}

		public T Get<T>(int index)
		{
			return (T)Convert.ChangeType(this[index], typeof(T));
		}

		private void AddValue(object value)
		{
			if(value == null)
			{
				throw new NullReferenceException("Attempted to add null value to list.");
			}
			if(value is bool b)
			{
				value = (byte)(b ? 1 : 0);
			}
			else if(value is INBTConverter i)
			{
				value = i.ToNBT(GameVersion.FirstVersion);
			}

			if(NBTMappings.GetTag(value.GetType()) != contentsType) throw new InvalidOperationException($"This ListContainer may only contain items of type '{contentsType}'.");
			listContent.Add(value);
			//return value;
		}

		public T Add<T>(T value)
		{
			AddValue(value);
			return value;
		}

		public void AddRange(params object[] values)
		{
			foreach(var value in values)
			{
				AddValue(value);
			}
		}

		public void RemoveAt(int index)
		{
			listContent.RemoveAt(index);
		}

		public object this[int i]
		{
			get { return listContent[i]; }
			set { listContent[i] = value; }
		}

		public override string[] GetContentKeys(string prefix = null)
		{
			string[] k = new string[listContent.Count];
			for(int i = 0; i < listContent.Count; i++) k[i] = prefix + i.ToString();
			return k;
		}

		public IEnumerator<object> GetEnumerator()
		{
			return listContent.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return listContent.GetEnumerator();
		}

		public List<T> ToList<T>()
		{
			var list = new List<T>();
			foreach(var item in listContent)
			{
				list.Add((T)item);
			}
			return list;
		}
	}
}

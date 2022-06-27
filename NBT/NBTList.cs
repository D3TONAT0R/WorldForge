using System;
using System.Collections;
using System.Collections.Generic;

namespace MCUtils.NBT
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
				return cont.Count;
			}
		}
		public List<object> cont = new List<object>();

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

		public object Get(int index)
		{
			return this[index];
		}

		public T Get<T>(int index)
		{
			return (T)Convert.ChangeType(this[index], typeof(T));
		}

		public void Add(object value)
		{
			if(value is bool b)
			{
				value = (byte)(b ? 1 : 0);
			}
			else if(value is INBTCompatible i)
			{
				value = i.GetNBTCompatibleObject();
			}

			if (NBTMappings.GetTag(value.GetType()) != contentsType) throw new InvalidOperationException($"This ListContainer may only contain items of type '{contentsType}'.");
			cont.Add(value);
			//return value;
		}

		public T Add<T>(T value)
		{
			Add(value);
			return value;
		}

		public void AddRange(params object[] values)
		{
			foreach (var value in values)
			{
				Add(value);
			}
		}

		public void RemoveAt(int index)
		{
			cont.RemoveAt(index);
		}

		public object this[int i]
		{
			get { return cont[i]; }
			set { cont[i] = value; }
		}

		public override string[] GetContentKeys(string prefix = null)
		{
			string[] k = new string[cont.Count];
			for (int i = 0; i < cont.Count; i++) k[i] = prefix + i.ToString();
			return k;
		}

		public IEnumerator<object> GetEnumerator()
		{
			return cont.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return cont.GetEnumerator();
		}
	}
}

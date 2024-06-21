using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.Entities
{

	public class Attribute : INBTConverter
	{
		[NBT("Name")]
		public string name;
		[NBT("Base")]
		public double baseValue;
		[NBT("Modifiers")]
		public List<NBTCompound> modifiers = null;

		public Attribute(string name, double baseValue)
		{
			this.name = name;
			this.baseValue = baseValue;
		}

		public Attribute(NBTCompound nbt)
		{
			FromNBT(nbt);
		}

		public void FromNBT(object nbtData)
		{
			NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
		}

		public object ToNBT(GameVersion version)
		{
			return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
		}
	}

	public class Attributes : INBTConverter
	{
		public List<Attribute> list;

		public Attributes()
		{
			list = new List<Attribute>();
		}

		/*
		public static Attributes CreateDefaultAttributes()
		{
			var attributes = new Attributes();
			attributes.Add("generic.maxHealth", 20);
			attributes.Add("generic.knockbackResistance", 0);
			attributes.Add("generic.movementSpeed", 0.1d);
			attributes.Add("generic.attackDamage", 1);
			attributes.Add("generic.armor", 0);
			attributes.Add("generic.attackSpeed", 4);
			attributes.Add("generic.luck", 0);
			return attributes;
		}
		*/

		public void Add(string name, double baseValue)
		{
			list.Add(new Attribute(name, baseValue));
		}

		public void FromNBT(object nbtData)
		{
			var list = (NBTList)nbtData;
			foreach(var comp in list)
			{
				this.list.Add(new Attribute((NBTCompound)comp));
			}
		}

		public object ToNBT(GameVersion version)
		{
			var nbtList = new NBTList(NBTTag.TAG_Compound);
			foreach(var a in list)
			{
				nbtList.Add(a.ToNBT(version));
			}
			return nbtList;
		}
	}

}

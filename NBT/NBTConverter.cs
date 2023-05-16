using System;
using System.Collections.Generic;
using System.Reflection;

namespace MCUtils.NBT
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class NBTAttribute : Attribute
	{

		public string tagName = null;
		public Version addedIn = Version.FirstVersion;
		public Version removedIn = new Version(Version.Stage.Release, 9, 9, 9);

		public NBTAttribute()
		{		
			
		}

		public NBTAttribute(string name)
		{
			tagName = name;
		}

		public NBTAttribute(string name, string addedInVersion) : this(name)
		{
			addedIn = Version.Parse(addedInVersion);
		}

		public NBTAttribute(string name, string addedInVersion, string removedInVersion) : this(name)
		{
			addedIn = Version.Parse(addedInVersion);
			removedIn = Version.Parse(removedInVersion);
		}
	}

	public class NBTConverter
	{
		public static void LoadFromNBT(NBTCompound sourceNBT, object target, bool removeFromCompound = false)
		{
			var type = target.GetType();
			foreach(var (fi, attr) in GetFields(target))
			{
				var name = !string.IsNullOrEmpty(attr.tagName) ? attr.tagName : fi.Name;
				if(sourceNBT.Contains(name))
				{
					object value;
					if (fi.FieldType == typeof(bool))
					{
						if(removeFromCompound) value = sourceNBT.Take<byte>(name) > 0;
						else value = sourceNBT.Get<byte>(name) > 0;
					}
					else if (typeof(INBTConverter).IsAssignableFrom(fi.FieldType))
					{
						var inst = Activator.CreateInstance(fi.FieldType);
						object data;
						if(removeFromCompound) data = sourceNBT.Take(name);
						else data = sourceNBT.Get(name);
						((INBTConverter)inst).FromNBT(data);
						value = inst;
					}
					else
					{
						if(removeFromCompound) value = sourceNBT.Take(name);
						else value = sourceNBT.Get(name);
					}
					fi.SetValue(target, value);
				}
			}
		}

		public static NBTCompound WriteToNBT(object source, NBTCompound targetNBT, Version targetVersion)
		{
			foreach(var (fi, attr) in GetFields(source))
			{
				if(targetVersion >= attr.addedIn && targetVersion < attr.removedIn)
				{
					var key = !string.IsNullOrEmpty(attr.tagName) ? attr.tagName : fi.Name;
					object value = fi.GetValue(source);
					if(value != null)
					{
						if(value is INBTConverter converter)
						{
							value = converter.ToNBT(targetVersion);
						}
						targetNBT.Add(key, value);
					}
				}
			}
			return targetNBT;
		}

		private static (FieldInfo fi, NBTAttribute attr)[] GetFields(object obj)
		{
			List<(FieldInfo fi, NBTAttribute attr)> list = new List<(FieldInfo fi, NBTAttribute attr)>();
			foreach(var fi in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				var attr = fi.GetCustomAttribute<NBTAttribute>();
				if(attr != null)
				{
					list.Add((fi, attr));
				}
			}
			return list.ToArray();
		}
	}
}

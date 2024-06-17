using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace WorldForge.NBT
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class NBTAttribute : Attribute
	{

		public string tagName = null;
		public GameVersion addedIn = GameVersion.FirstVersion;
		public GameVersion removedIn = new GameVersion(GameVersion.Stage.Release, 9, 9, 9);

		public NBTAttribute()
		{

		}

		public NBTAttribute(string name)
		{
			tagName = name;
		}

		public NBTAttribute(string name, string addedInVersion) : this(name)
		{
			addedIn = GameVersion.Parse(addedInVersion);
		}

		public NBTAttribute(string name, string addedInVersion, string removedInVersion) : this(name)
		{
			addedIn = GameVersion.Parse(addedInVersion);
			removedIn = GameVersion.Parse(removedInVersion);
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
					try
					{
						object value;
						if(fi.FieldType == typeof(bool))
						{
							if(removeFromCompound) value = sourceNBT.Take<byte>(name) > 0;
							else value = sourceNBT.Get<byte>(name) > 0;
						}
						else if(typeof(INBTConverter).IsAssignableFrom(fi.FieldType))
						{
							var inst = Activator.CreateInstance(fi.FieldType);
							object data;
							if(removeFromCompound) data = sourceNBT.Take(name);
							else data = sourceNBT.Get(name);
							((INBTConverter)inst).FromNBT(data);
							value = inst;
						}
						else if(fi.FieldType.IsGenericType && fi.FieldType.GetGenericTypeDefinition() == typeof(List<>))
						{
							var list = Activator.CreateInstance(fi.FieldType);
							var addMethod = fi.FieldType.GetMethod("Add");

							NBTList nbtList;
							if(removeFromCompound) nbtList = sourceNBT.Take<NBTList>(name);
							else nbtList = sourceNBT.Get<NBTList>(name);
							foreach(var item in nbtList)
							{
								addMethod.Invoke(list, new object[] { item });
							}
							value = list;
						}
						else
						{
							if(removeFromCompound) value = sourceNBT.Take(name);
							else value = sourceNBT.Get(name);
						}
						fi.SetValue(target, value);
					}
					catch(Exception e)
					{
						throw new Exception("Failed to set value: " + name, e);
					}
				}
			}
		}

		public static NBTCompound WriteToNBT(object source, NBTCompound targetNBT, GameVersion targetVersion)
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

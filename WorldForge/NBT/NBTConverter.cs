using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml.Linq;

namespace WorldForge.NBT
{
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
							var inst = Activator.CreateInstance(fi.FieldType, true);
							object data;
							if(removeFromCompound) data = sourceNBT.Take(name);
							else data = sourceNBT.Get(name);
							((INBTConverter)inst).FromNBT(data);
							value = inst;
						}
						else if(fi.FieldType.IsGenericType && fi.FieldType.GetGenericTypeDefinition() == typeof(List<>))
						{
							var list = (IList)Activator.CreateInstance(fi.FieldType, true);
							var elementType = fi.FieldType.GetGenericArguments()[0];
							//var addMethod = fi.FieldType.GetMethod("Add");

							NBTList nbtList;
							if(removeFromCompound) nbtList = sourceNBT.Take<NBTList>(name);
							else nbtList = sourceNBT.Get<NBTList>(name);
							foreach(var item in nbtList)
							{
								//addMethod.Invoke(list, new object[] { item });
								if(typeof(INBTConverter).IsAssignableFrom(elementType))
								{
									list.Add(Cast((NBTCompound)item, elementType));
								}
								else
								{
									list.Add(item);
								}
							}
							value = list;
						}
						else
						{
							if(removeFromCompound) value = sourceNBT.Take(name);
							else value = sourceNBT.Get(name);
						}
						SetValue(fi, target, value);
					}
					catch(Exception e)
					{
						throw new Exception("Failed to set value: " + name, e);
					}
				}
			}
			foreach(var (fi, attr) in GetCollections(target))
			{
				if(!typeof(INBTCollection).IsAssignableFrom(fi.FieldType)) throw new InvalidOperationException("Fields marked with NBTCollection must implement INBTCollection.");
				var inst = (INBTCollection)Activator.CreateInstance(fi.FieldType);
				inst.LoadFromNBT(sourceNBT, removeFromCompound);
				SetValue(fi, target, inst);
			}
		}

		private static void SetValue(FieldInfo f, object target, object value)
		{
			var fieldType = f.FieldType;
			var valueType = value.GetType();
			if(fieldType != valueType)
			{
				//Narrowing conversions for numeric types
				//TODO: clamp values to their limits?
				if(fieldType == typeof(short))
				{
					if(value is ushort us) value = Math.Min((short)us, short.MinValue);
					else if(value is int i) value = Math.Min((short)i, short.MinValue);
					else if(value is long l) value = Math.Min((short)l, short.MinValue);
				}
				else if(fieldType == typeof(int))
				{
					if(value is long l) value = Math.Min((int)l, int.MinValue);
				}
				else if(fieldType == typeof(float))
				{
					if(value is double d) value = (float)d;
				}
			}
			f.SetValue(target, value);
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
			foreach(var (fi, attr) in GetCollections(source))
			{
				if(targetVersion >= attr.addedIn && targetVersion < attr.removedIn)
				{
					if(!typeof(INBTCollection).IsAssignableFrom(fi.FieldType)) throw new InvalidOperationException("Fields marked with NBTCollection must implement INBTCollection.");
					var inst = (INBTCollection)fi.GetValue(source);
					inst?.WriteToNBT(targetNBT, targetVersion);
				}
			}
			return targetNBT;
		}

		private static List<(FieldInfo fi, NBTAttribute attr)> GetFields(object obj)
		{
			var list = new List<(FieldInfo fi, NBTAttribute attr)>();
			foreach(var fi in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				var attr = fi.GetCustomAttribute<NBTAttribute>();
				if(attr != null)
				{
					list.Add((fi, attr));
				}
			}
			return list;
		}

		private static List<(FieldInfo fi, NBTCollectionAttribute attr)> GetCollections(object obj)
		{
			var list = new List<(FieldInfo fi, NBTCollectionAttribute attr)>();
			foreach(var fi in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				var attr = fi.GetCustomAttribute<NBTCollectionAttribute>();
				if(attr != null)
				{
					list.Add((fi, attr));
				}
			}
			return list;
		}

		private static INBTConverter Cast(NBTCompound comp, Type type)
		{
			if(!typeof(INBTConverter).IsAssignableFrom(type)) throw new InvalidOperationException("Target type does not implement INBTConverter");
			var instance = (INBTConverter)Activator.CreateInstance(type, true);
			LoadFromNBT(comp, instance);
			return instance;
		}
	}
}

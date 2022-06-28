using System;
using System.Collections.Generic;

namespace MCUtils.NBT
{
	public enum NBTTag
	{
		TAG_End = 0,
		TAG_Byte = 1,
		TAG_Short = 2,
		TAG_Int = 3,
		TAG_Long = 4,
		TAG_Float = 5,
		TAG_Double = 6,
		TAG_Byte_Array = 7,
		TAG_String = 8,
		TAG_List = 9,
		TAG_Compound = 10,
		TAG_Int_Array = 11,
		TAG_Long_Array = 12,
		UNSPECIFIED = 99
	}

	public static class NBTMappings
	{
		public static NBTTag GetTag(Type t)
		{
			if(NBTTagDictionary.TryGetValue(t, out var tag))
			{
				return tag;
			}
			else
			{
				throw new NotSupportedException($"Type '{t}' is not supported.");
			}
		}

		private static Dictionary<Type, NBTTag> NBTTagDictionary = new Dictionary<Type, NBTTag> {
			{ typeof(byte), NBTTag.TAG_Byte },
			{ typeof(short), NBTTag.TAG_Short },
			{ typeof(int), NBTTag.TAG_Int },
			{ typeof(long), NBTTag.TAG_Long },
			{ typeof(float), NBTTag.TAG_Float },
			{ typeof(double), NBTTag.TAG_Double },
			{ typeof(byte[]), NBTTag.TAG_Byte_Array },
			{ typeof(string), NBTTag.TAG_String },
			{ typeof(NBTList), NBTTag.TAG_List },
			{ typeof(NBTCompound), NBTTag.TAG_Compound },
			{ typeof(int[]), NBTTag.TAG_Int_Array },
			{ typeof(long[]), NBTTag.TAG_Long_Array }
		};
	}
}

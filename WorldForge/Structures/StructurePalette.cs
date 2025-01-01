using System.Collections.Generic;
using System.Linq;
using WorldForge.NBT;

namespace WorldForge.Structures
{
	public class StructurePalette<T> : IStructurePalette<T> where T : INBTConverter, new ()
	{
		public readonly List<T> list = new List<T>();

		public int PaletteCount => 1;

		public StructurePalette()
		{

		}

		public StructurePalette(NBTList nbt)
		{
			FromNBT(nbt);
		}

		public StructurePalette(params T[] values)
		{
			list.AddRange(values);
		}

		public List<T> GetPalette(int paletteIndex = 0) => list;

		public object ToNBT(GameVersion version) => new NBTList(NBTTag.TAG_Compound, list.Select(t => t.ToNBT(version)));

		public void FromNBT(object nbtData)
		{
			var nbtList = (NBTList)nbtData;
			list.Clear();
			for(var i = 0; i < nbtList.Length; i++)
			{
				var item = new T();
				item.FromNBT(nbtList.Get<NBTCompound>(i));
				list.Add(item);
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using WorldForge.NBT;

namespace WorldForge.Structures
{
	public class StructureMultiPalette<T> : IStructurePalette<T> where T : INBTConverter, new()
	{
		public readonly List<List<T>> palettes = new List<List<T>>();

		public int PaletteCount => palettes.Count;

		public StructureMultiPalette()
		{

		}

		public StructureMultiPalette(NBTList nbt)
		{
			FromNBT(nbt);
		}

		public List<T> GetPalette(int paletteIndex = 0) => palettes[paletteIndex];

		public object ToNBT(GameVersion version)
		{
			var palettesList = new NBTList(NBTTag.TAG_List);
			foreach(var palette in palettes)
			{
				palettesList.Add(new NBTList(NBTTag.TAG_Compound, palette.Select(t => t.ToNBT(version))));
			}
			return palettesList;
		}

		public void FromNBT(object nbtData)
		{
			var nbtList = (NBTList)nbtData;
			palettes.Clear();
			for(var i = 0; i < nbtList.Length; i++)
			{
				var palette = new List<T>();
				var paletteNbt = nbtList.Get<NBTList>(i);
				for(int j = 0; j < paletteNbt.Length; j++)
				{
					var state = new T();
					state.FromNBT(paletteNbt.Get<NBTCompound>(j));
					palette.Add(state);
				}
				palettes.Add(palette);
			}
		}
	}
}

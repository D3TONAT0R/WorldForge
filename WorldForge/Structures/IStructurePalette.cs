using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Structures
{
	public interface IStructurePalette<T> : INBTConverter where T : INBTConverter, new()
	{
		int PaletteCount { get; }

		List<T> GetPalette(int paletteIndex = 0);
	}
}
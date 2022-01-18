using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class ChunkSection
	{
		public ushort[,,] blocks = new ushort[16, 16, 16];
		public List<BlockState> palette;

		//private readonly object lockObj = new object();

		public ChunkSection(string defaultBlock)
		{
			palette = new List<BlockState>();
			if (defaultBlock != null)
			{
				palette.Add(new BlockState(BlockList.Find("minecraft:air"))); //Index 0
				palette.Add(new BlockState(BlockList.Find(defaultBlock))); //Index 1
			}
		}

		public void SetBlockAt(int x, int y, int z, BlockState block)
		{
			//lock (lockObj)
			//{
				ushort? index = GetPaletteIndex(block);
				if (index == null)
				{
					index = AddBlockToPalette(block);
				}
				blocks[x, y, z] = (ushort)index;
			//}
		}

		public void SetBlockAt(int x, int y, int z, ushort paletteIndex)
		{
			//lock (lockObj)
			//{
				blocks[x, y, z] = paletteIndex;
			//}
		}

		public BlockState GetBlockAt(int x, int y, int z)
		{
			return palette[blocks[x, y, z]];
		}

		public ushort? GetPaletteIndex(BlockState state)
		{
			for (short i = 0; i < palette.Count; i++)
			{
				if (palette[i].Compare(state, true)) return (ushort)i;
			}
			return null;
		}

		private ushort AddBlockToPalette(BlockState block)
		{
			if (block == null) throw new NullReferenceException("Attempted to add a null BlockState to the palette.");
			palette.Add(block);
			return (ushort)(palette.Count - 1);
		}

		public bool IsEmpty()
		{
			if (blocks == null) return true;
			bool allSame = true;
			var i = blocks[0, 0, 0];
			if (!palette[i].Compare(BlockState.Air, false)) return false;
			foreach (var j in blocks)
			{
				allSame &= i == j;
			}
			return allSame;
		}

		public CompoundContainer CreateCompound(sbyte secY, bool use_1_16_Format)
		{
			var comp = new CompoundContainer();
			comp.Add("Y", (byte)secY);
			ListContainer paletteContainer = new ListContainer(NBTTag.TAG_Compound);
			foreach (var block in palette)
			{
				CompoundContainer paletteBlock = new CompoundContainer();
				paletteBlock.Add("Name", block.block.ID);
				if (block.properties != null)
				{
					CompoundContainer properties = new CompoundContainer();
					foreach (var prop in block.properties.cont.Keys)
					{
						properties.Add(prop, block.properties.Get(prop).ToString());
					}
					paletteBlock.Add("Properties", properties);
				}
				paletteContainer.Add("", paletteBlock);
			}
			comp.Add("Palette", paletteContainer);
			//Encode block indices to bits and longs, oof
			int indexLength = Math.Max(4, (int)Math.Log(palette.Count - 1, 2.0) + 1);
			//How many block indices fit inside a long?
			int indicesPerLong = (int)Math.Floor(64f / indexLength);
			long[] longs = new long[(int)Math.Ceiling(4096f / indicesPerLong)];
			string[] longsBinary = new string[longs.Length];
			for (int j = 0; j < longsBinary.Length; j++)
			{
				longsBinary[j] = "";
			}
			int i = 0;
			for (int y = 0; y < 16; y++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int x = 0; x < 16; x++)
					{
						string bin = NumToBits(blocks[x, y, z], indexLength);
						bin = Converter.ReverseString(bin);
						if (use_1_16_Format)
						{
							if (longsBinary[i].Length + indexLength > 64)
							{
								//The full value doesn't fit, start on the next long
								i++;
								longsBinary[i] += bin;
							}
							else
							{
								for (int j = 0; j < indexLength; j++)
								{
									if (longsBinary[i].Length >= 64) i++;
									longsBinary[i] += bin[j];
								}
							}
						}
					}
				}
			}
			for (int j = 0; j < longs.Length; j++)
			{
				string s = longsBinary[j];
				s = s.PadRight(64, '0');
				s = Converter.ReverseString(s);
				longs[j] = Convert.ToInt64(s, 2);
			}
			comp.Add("BlockStates", longs);
			return comp;
		}

		private static string NumToBits(ushort num, int length)
		{
			string s = Convert.ToString(num, 2);
			if (s.Length > length)
			{
				throw new IndexOutOfRangeException("The number " + num + " does not fit in a binary string with length " + length);
			}
			return s.PadLeft(length, '0');
		}
	}
}

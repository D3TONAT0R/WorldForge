using MCUtils.NBT;
using System;

namespace MCUtils.IO
{
	public class ChunkSerializer_1_13 : ChunkSerializerAnvil
	{
		public ChunkSerializer_1_13(Version version) : base(version) { }

		public virtual bool UseFull64BitRange => true;

		public override void WriteCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			base.WriteCommonData(c, chunkNBT);

			//TODO: find out in which version these tags were added
			chunkNBT.Add("Status", c.status.ToString());
			chunkNBT.Add("InhabitedTime", c.inhabitedTime);

			//Leave it empty (or implement heightmap gen in the future?)
			chunkNBT.Add("Heightmaps", new NBTCompound());

			chunkNBT.Add("Structures", new NBTCompound());
		}

		protected virtual string PaletteBlockNameKey => "Name";
		protected virtual string PaletteBlockPropertiesKey => "Properties";
		protected virtual string PaletteKey => "Palette";
		protected virtual string BlockStatesKey => "BlockStates";

		public override void WriteSection(ChunkSection section, NBTCompound comp, sbyte sectionY)
		{
			NBTList paletteList = new NBTList(NBTTag.TAG_Compound);
			foreach(var block in section.palette)
			{
				NBTCompound paletteBlock = new NBTCompound();
				paletteBlock.Add(PaletteBlockNameKey, block.block.ID);
				if(block.properties != null)
				{
					NBTCompound properties = new NBTCompound();
					foreach(var prop in block.properties.contents.Keys)
					{
						var value = block.properties.Get(prop);
						if(value is bool b) value = b.ToString().ToLower();
						properties.Add(prop, value.ToString());
					}
					paletteBlock.Add(PaletteBlockPropertiesKey, properties);
				}
				paletteList.Add(paletteBlock);
			}
			comp.Add(PaletteKey, paletteList);
			//Encode block indices to bits and longs
			int indexLength = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
			//How many block indices fit inside a long?
			//TODO: 1.13 and 1.16 should use different amounts
			int indicesPerLong = (int)Math.Floor(64f / indexLength);
			long[] longs = new long[(int)Math.Ceiling(4096f / indicesPerLong)];
			string[] longsBinary = new string[longs.Length];
			for(int j = 0; j < longsBinary.Length; j++)
			{
				longsBinary[j] = "";
			}
			int i = 0;
			for(int y = 0; y < 16; y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x++)
					{
						string bin = NumToBits(section.blocks[x, y, z], indexLength);
						bin = Converter.ReverseString(bin);
						if(!UseFull64BitRange)
						{
							if(longsBinary[i].Length + indexLength > 64)
							{
								//The full value doesn't fit, start on the next long
								i++;
								longsBinary[i] += bin;
							}
							else
							{
								for(int j = 0; j < indexLength; j++)
								{
									if(longsBinary[i].Length >= 64) i++;
									longsBinary[i] += bin[j];
								}
							}
						}
					}
				}
			}
			for(int j = 0; j < longs.Length; j++)
			{
				string s = longsBinary[j];
				s = s.PadRight(64, '0');
				s = Converter.ReverseString(s);
				longs[j] = Convert.ToInt64(s, 2);
			}
			comp.Add(BlockStatesKey, longs);
		}

		public override void LoadCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			if(chunkNBT.TryGet("Status", out string statusString))
			{
				Enum.TryParse(statusString, out c.status);
			}
			else if(chunkNBT.TryGet("status", out statusString))
			{
				Enum.TryParse(statusString, out c.status);
			}
		}

		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach (var o in sectionsList.listContent)
			{
				var section = new ChunkSection(c, null);

				var sectionComp = (NBTCompound)o;
				if (!HasBlocks(sectionComp)) continue;
				sbyte secY = ParseSectionYIndex(sectionComp);

				section.palette.Clear();
				foreach (var cont in GetBlockPalette(sectionComp).listContent)
				{
					var paletteItem = (NBTCompound)cont;
					section.palette.Add(ParseBlockState(paletteItem));
				}

				if (section.palette.Count == 1)
				{
					//Do nothing, as all blocks already have an index of 0
				}
				else
				{
					//1.15 and prior uses the full range of bits where 1.16 and later doesn't use the last bits if they can't contain a block index
					int indexBitCount = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
					long[] longs = GetBlockDataArray(sectionComp);

					ushort[] indices = BitUtils.ExtractCompressedInts(longs, indexBitCount, 4096, UseFull64BitRange);

					for (int y = 0; y < 16; y++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int x = 0; x < 16; x++)
							{
								section.blocks[x, y, z] = indices[y * 256 + z * 16 + x];
							}
						}
					}
				}
				c.sections.Add(secY, section);
			}
		}

		protected override bool HasBlocks(NBTCompound sectionNBT)
		{
			return sectionNBT.Contains("Palette");
		}

		protected virtual NBTList GetBlockPalette(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsList("Palette");
		}

		protected virtual BlockState ParseBlockState(NBTCompound paletteItemNBT)
		{
			var proto = BlockList.Find((string)paletteItemNBT.Get("Name"));
			if (proto != null)
			{
				var blockState = new BlockState(proto);
				if (paletteItemNBT.Contains("Properties")) blockState.properties = paletteItemNBT.GetAsCompound("Properties");
				return blockState;
			}
			else
			{
				return BlockState.Unknown;
			}
		}

		protected virtual long[] GetBlockDataArray(NBTCompound sectionNBT)
		{
			return sectionNBT.Get<long[]>("BlockStates");
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			if (chunkNBT.TryGet<int[]>("Biomes", out var biomeData))
			{
				for (int y = 0; y < 64; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						for (int z = 0; z < 4; z++)
						{
							int i = y * 16 + z * 4 + x;
							var biome = (BiomeID)biomeData[i];
							var section = c.GetChunkSectionForYCoord(y * 4, false);
							if (section != null)
							{
								section.SetBiome3D4x4At(x * 4, y * 4, z * 4, biome);
							}
						}
					}
				}
			}
		}

		public override void LoadTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//TODO
		}

		public override void WriteTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//Add "post processing" positions (i.e. block positions that need an update)
			var ppList = chunkNBT.Add("PostProcessing", new NBTList(NBTTag.TAG_List));
			for (int i = 0; i < 16; i++)
			{
				ppList.Add(new NBTList(NBTTag.TAG_Short));
			}

			foreach (var t in c.postProcessTicks)
			{
				int listIndex = t.y / 16;
				var x = t.x.Mod(16);
				var y = t.y.Mod(16);
				var z = t.z.Mod(16);
				var list = ppList.Get<NBTList>(listIndex);
				short packed = (short)((z << 8) + (y << 4) + x);
				list.Add(packed);
			}
		}

		private static string NumToBits(ushort num, int length)
		{
			string s = Convert.ToString(num, 2);
			if(s.Length > length)
			{
				throw new IndexOutOfRangeException("The number " + num + " does not fit in a binary string with length " + length);
			}
			return s.PadLeft(length, '0');
		}
	}
}

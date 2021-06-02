using System;
using System.Collections.Generic;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class ChunkData
	{

		public class ChunkSection
		{
			public ushort[,,] blocks = new ushort[16, 16, 16];
			public List<BlockState> palette;

			public ChunkSection(string defaultBlock)
			{
				palette = new List<BlockState>();
				if (defaultBlock != null)
				{
					palette.Add(new BlockState("minecraft:air")); //Index 0
					palette.Add(new BlockState(defaultBlock)); //Index 1
				}
			}

			public void SetBlockAt(int x, int y, int z, BlockState block)
			{
				ushort? index = GetPaletteIndex(block);
				if (index == null)
				{
					index = AddBlockToPalette(block);
				}
				blocks[x, y, z] = (ushort)index;
			}

			public void SetBlockAt(int x, int y, int z, ushort paletteIndex)
			{
				blocks[x, y, z] = paletteIndex;
			}

			public BlockState GetBlockAt(int x, int y, int z)
			{
				return palette[blocks[x, y, z]];
			}

			public ushort? GetPaletteIndex(BlockState state)
			{
				for (short i = 0; i < palette.Count; i++)
				{
					if (palette[i].ID == state.ID && palette[i].properties.HasSameContent(state.properties)) return (ushort)i;
				}
				return null;
			}

			public ushort AddBlockToPalette(BlockState block)
			{
				palette.Add(block);
				return (ushort)(palette.Count - 1);
			}

			private bool IsEmpty()
			{
				if (blocks == null) return true;
				bool allSame = true;
				var i = blocks[0, 0, 0];
				if (!palette[i].Compare(BlockState.air, false)) return false;
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
					paletteBlock.Add("Name", block.ID);
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
		}

		public class BlockState
		{

			public static readonly BlockState air = new BlockState("minecraft:air");

			public readonly string ID;
			public readonly string customNamespace = null;
			public readonly string shortID;
			public CompoundContainer properties = new CompoundContainer();

			public BlockState(string name)
			{
				if (!name.Contains(":"))
				{
					name = "minecraft:" + name;
				}
				ID = name;
				var split = ID.Split(':');
				customNamespace = split[0] == "minecraft" ? null : split[0];
				name = split[1];
				shortID = name;
				AddDefaultBlockProperties();
			}

			void AddDefaultBlockProperties()
			{
				switch (shortID)
				{
					case "oak_leaves":
					case "spruce_leaves":
					case "birch_leaves":
					case "jungle_leaves":
					case "acacia_leaves":
					case "dark_oak_leaves":
						properties.Add("distance", 1);
						break;
				}
			}

			public bool CompareMultiple(params string[] ids)
			{
				bool b = false;
				for (int i = 0; i < ids.Length; i++)
				{
					b |= Compare(ids[i]);
				}
				return b;
			}

			public bool Compare(string block)
			{
				return block == ID;
			}

			public bool Compare(BlockState state, bool compareProperties)
			{
				if (compareProperties)
				{
					if (!CompoundContainer.AreEqual(properties, state.properties)) return false;
				}
				return state.ID == ID;
			}
		}

		public string defaultBlock = "minecraft:stone";
		public bool unlimitedHeight = false; //Allow blocks below 0 and above 256 (Versions 1.17+)
		public Region containingRegion;
		public bool hasNumericIDs;
		public Dictionary<sbyte, ChunkSection> sections = new Dictionary<sbyte, ChunkSection>();
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }
		//public ushort[][,,] blocks = new ushort[16][,,];
		//public List<BlockState>[] palettes = new List<BlockState>[16];
		public byte[,] biomes = new byte[16, 16];
		public int[,,] finalBiomeArray;

		public List<Entity> entities = new List<Entity>();
		public Dictionary<(int x, int y, int z), TileEntity> tileEntities = new Dictionary<(int, int, int), TileEntity>();

		public NBTContent sourceNBT;

		public ChunkData(Region region, string defaultBlock)
		{
			containingRegion = region;
			this.defaultBlock = defaultBlock;
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					biomes[x, y] = 1; //Defaults to plains biome
				}
			}
		}

		public ChunkData(Region region, NBTContent chunk)
		{
			containingRegion = region;
			//for(int i = 0; i < 16; i++) {
			//	palettes[i] = new List<BlockState>();
			//}
			ReadFromNBT(chunk.contents.GetAsList("Sections"), chunk.dataVersion < 2504);
			if (chunk.dataVersion < 1400)
			{
				hasNumericIDs = true;
			}
			sourceNBT = chunk;
		}

		///<summary>Sets the block at the given chunk coordinate</summary>
		public void SetBlockAt(int x, int y, int z, BlockState block)
		{
			if (!unlimitedHeight && (y < 0 || y > 255)) return;
			if (hasNumericIDs)
			{
				Console.WriteLine("Changing blocks in a numeric ID chunk is currently not supported.");
				return;
			}
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, block);
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)Math.Floor(y / 16f);
			if (!sections.ContainsKey(sectionY))
			{
				if (allowNew)
				{
					sections.Add(sectionY, new ChunkSection(defaultBlock));
					RecalculateSectionRange();
				}
				else
				{
					return null;
				}
			}
			return sections[sectionY];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given chunk coordinate. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlockAt(int x, int y, int z)
		{
			if (hasNumericIDs)
			{
				Console.WriteLine("Changing blocks in a numeric ID chunk is currently not supported.");
				return;
			}
			int section = (int)Math.Floor(y / 16f);
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, 1); //1 is always the default block in a region generated from scratch
		}

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlockAt(int x, int y, int z)
		{
			var sec = GetChunkSectionForYCoord(y, false);
			if (sec == null) return BlockState.air;
			return sec.GetBlockAt(x, y % 16, z);
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(int x, int y, int z, TileEntity te)
		{
			if (tileEntities == null)
			{
				tileEntities = new Dictionary<(int x, int y, int z), TileEntity>();
			}
			tileEntities.Add((x, y, z), te);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(int x, int y, int z)
		{
			if (tileEntities == null) return null;
			if (tileEntities.ContainsKey((x, y, z)))
			{
				return tileEntities[(x, y, z)];
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int z, byte biomeID)
		{
			biomes[x, z] = biomeID;
		}

		///<summary>Reads all blocks in the given chunk</summary>
		public void ReadFromNBT(ListContainer sectionsList, bool isVersion_prior_1_16)
		{
			foreach (var o in sectionsList.cont)
			{
				var section = new ChunkSection(null);

				var compound = (CompoundContainer)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				sbyte secY;
				unchecked
				{
					secY = Convert.ToSByte(compound.Get("Y"));
				}
				section.palette.Clear();
				foreach (var cont in compound.GetAsList("Palette").cont)
				{
					CompoundContainer block = (CompoundContainer)cont;
					BlockState bs = new BlockState((string)block.Get("Name"));
					if (block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
					section.palette.Add(bs);
				}

				//1.15 uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
				int indexLength = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
				long[] longs = (long[])compound.Get("BlockStates");
				string bits = "";
				for (int i = 0; i < longs.Length; i++)
				{
					string newBits = "";
					byte[] bytes = BitConverter.GetBytes(longs[i]);
					for (int j = 0; j < 8; j++)
					{
						newBits += Converter.ByteToBinary(bytes[j], true);
					}
					if (isVersion_prior_1_16)
					{
						bits += newBits;
					}
					else
					{
						bits += newBits.Substring(0, (int)Math.Floor(newBits.Length / (double)indexLength) * indexLength);
					}
				}
				//TODO: needs testing
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x++)
						{
							section.blocks[x, y, z] = Converter.BitsToValue(bits, y * 256 + z * 16 + x, indexLength);
						}
					}
				}
				sections.Add(secY, section);
				RecalculateSectionRange();
			}
		}

		///<summary>Converts the two-dimensional per-block biome array into a Minecraft compatible biome array (4x4x4 block volumes)</summary>
		public void MakeBiomeArray()
		{
			finalBiomeArray = new int[4, 64, 4];
			for (int x = 0; x < 4; x++)
			{
				for (int z = 0; z < 4; z++)
				{
					int biome = GetPredominantBiomeIn4x4Area(x, z);
					for (int y = 0; y < 64; y++) finalBiomeArray[x, y, z] = biome;
				}
			}
		}

		public short GetHighestBlock(int chunkX, int chunkZ, HeightmapType type)
		{
			short y = (short)(HighestSection * 16 + 15);
			while (y > LowestSection * 16)
			{
				if (Blocks.IsBlockForMap(GetBlockAt(chunkX, y, chunkZ), type)) return y;
				y--;
			}
			return short.MinValue;
		}

		public short GetHighestBlock(int chunkX, int chunkZ)
		{
			return GetHighestBlock(chunkX, chunkZ, HeightmapType.AllBlocks);
		}

		///<summary>Generates the full NBT data of a chunk</summary>
		public void WriteToNBT(CompoundContainer level, bool use_1_16_Format)
		{
			ListContainer sectionsList = level.GetAsList("Sections");
			foreach (sbyte secY in sections.Keys)
			{
				var section = sections[secY];
				//if(IsSectionEmpty(secY)) continue;
				var comp = GetSectionCompound(sectionsList, secY);
				if (comp == null)
				{
					comp = section.CreateCompound(secY, use_1_16_Format);
				}
				sectionsList.Add(null, comp);
			}
			//Make the biomes
			List<int> biomes = new List<int>();
			for (int y = 0; y < 64; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					for (int z = 0; z < 4; z++)
					{
						var b = finalBiomeArray != null ? finalBiomeArray[x, y, z] : 1;
						biomes.Add(b);
					}
				}
			}
			level.Add("Biomes", biomes.ToArray());
		}

		///<summary>Writes the chunk's height data to a large heightmap at the given chunk coords</summary>
		public void WriteToHeightmap(short[,] hm, int x, int z, HeightmapType type)
		{
			if (!WriteHeightmapFromNBT(hm, x, z, type))
			{
				WriteHeightmapFromBlocks(hm, x, z, type);
			}
		}

		private void RecalculateSectionRange()
		{
			sbyte? lowest = null;
			sbyte? highest = null;
			for (sbyte s = -100; s < 127; s++)
			{
				if (lowest == null && sections.ContainsKey(s))
				{
					lowest = s;
				}
				if (sections.ContainsKey(s))
				{
					highest = s;
				}
			}
			LowestSection = lowest ?? 0;
			HighestSection = highest ?? 0;
		}

		private bool WriteHeightmapFromNBT(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			if (sourceNBT == null) return false;
			var chunkHM = sourceNBT.GetHeightmapFromChunkNBT(type);
			if (chunkHM == null) return false;
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = chunkHM[x, z];
				}
			}
			return true;
		}

		private void WriteHeightmapFromBlocks(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			sbyte highestSection = 127;
			while (highestSection > -127 && !sections.ContainsKey(highestSection))
			{
				highestSection--;
			}
			if (highestSection == -127) return;
			var sec = sections[highestSection];
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					short yTop;
					if (hm[x, z] != 0)
					{
						//The heightmap is already present, proceed from provided y value
						yTop = hm[x, z];
					}
					else
					{
						yTop = (short)(highestSection * 16 + 15);
					}
					for (short y = yTop; y > 0; y--)
					{
						var block = sec.GetBlockAt(x, y % 16, z);
						if (Blocks.IsBlockForMap(block, type))
						{
							if(block.Compare("minecraft:snow"))
							{
								//Go down to grass level in case of a snow layer
								y--;
							}
							hm[localChunkX * 16 + x, 511 - (localChunkZ * 16 + z)] = y;
							break;
						}
					}
				}
			}
		}

		private long BitsToLong(string bits)
		{
			bits = bits.PadLeft(64, '0');
			return Convert.ToInt64(bits, 2);
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

		//TODO: How to deal with negative section Y values? (Minecraft 1.17+)
		private CompoundContainer GetSectionCompound(ListContainer sectionsList, sbyte y)
		{
			foreach (var o in sectionsList.cont)
			{
				var compound = (CompoundContainer)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				if ((byte)compound.Get("Y") == y) return compound;
			}
			return null;
		}

		private int GetPredominantBiomeIn4x4Area(int x, int z)
		{
			Dictionary<byte, byte> occurences = new Dictionary<byte, byte>();
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int z1 = 0; z1 < 4; z1++)
				{
					var b = biomes[x * 4 + x1, z * 4 + z1];
					if (!occurences.ContainsKey(b))
					{
						occurences.Add(b, 0);
					}
					occurences[b]++;
				}
			}
			int predominantBiome = 0;
			int predominantCells = 0;
			foreach (var k in occurences.Keys)
			{
				if (occurences[k] > predominantCells)
				{
					predominantCells = occurences[k];
					predominantBiome = k;
				}
			}
			return predominantBiome;
		}
	}
}
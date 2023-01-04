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
			chunkNBT.Add("Status", "light");
			chunkNBT.Add("InhabitedTime", c.inhabitedTime);

			//Leave it empty (or implement heightmap gen in the future?)
			chunkNBT.Add("Heightmaps", new NBTCompound());

			chunkNBT.Add("Structures", new NBTCompound());
		}

		public override void WriteSection(ChunkSection c, NBTCompound sectionNBT, sbyte sectionY)
		{
			c.CreateCompound(sectionNBT, sectionY, false);
		}

		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach (var o in sectionsList.listContent)
			{
				var section = new ChunkSection(null);

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

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			int[] biomeData = new int[1024];
			for (int y = 0; y < 64; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					for (int z = 0; z < 4; z++)
					{
						int i = y * 16 + z * 4 + x;
						var section = c.GetChunkSectionForYCoord(y * 4, false);
						if (section != null)
						{
							biomeData[i] = (int)section.GetPredominantBiomeAt4x4(x, y % 4, z);
						}
						else
						{
							biomeData[i] = (int)BiomeID.plains;
						}
					}
				}
			}
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
				var x = t.x % 16;
				var y = t.y % 16;
				var z = t.z % 16;
				var list = ppList.Get<NBTList>(listIndex);
				short packed = (short)((z << 8) + (y << 4) + x);
				list.Add(packed);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using MCUtils;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class ChunkSerializer_1_13 : ChunkSerializerAnvil
	{
		public ChunkSerializer_1_13(Version version) : base(version) { }

		public override void LoadBlocks(ChunkData c, CompoundContainer nbtCompound)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach (var o in sectionsList.cont)
			{
				var section = new ChunkSection(null);

				var sectionComp = (CompoundContainer)o;
				if (!HasBlocks(sectionComp)) continue;
				sbyte secY = ParseSectionYIndex(sectionComp);

				section.palette.Clear();
				foreach (var cont in GetBlockPalette(sectionComp).cont)
				{
					var paletteItem = (CompoundContainer)cont;
					section.palette.Add(ParseBlockState(paletteItem));
				}

				if (section.palette.Count == 1)
				{
					//Do nothing, as all blocks already have an index of 0
				}
				else
				{
					//1.15 and prior uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
					int indexBitCount = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
					long[] longs = GetBlockDataArray(sectionComp);
					string bits = "";
					for (int i = 0; i < longs.Length; i++)
					{
						string newBits = "";
						byte[] bytes = BitConverter.GetBytes(longs[i]);
						for (int j = 0; j < 8; j++)
						{
							newBits += Converter.ByteToBinary(bytes[j], true);
						}
						bits = AppendBlockStateBitsToBitStream(bits, newBits, indexBitCount);
					}
					for (int y = 0; y < 16; y++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int x = 0; x < 16; x++)
							{
								section.blocks[x, y, z] = Converter.BitsToValue(bits, y * 256 + z * 16 + x, indexBitCount);
							}
						}
					}
				}
				c.sections.Add(secY, section);
			}
		}

		protected override bool HasBlocks(CompoundContainer sectionNBT)
		{
			return sectionNBT.Contains("Palette");
		}

		protected virtual ListContainer GetBlockPalette(CompoundContainer sectionNBT)
		{
			return sectionNBT.GetAsList("Palette");
		}

		protected virtual BlockState ParseBlockState(CompoundContainer paletteItemNBT)
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

		protected virtual long[] GetBlockDataArray(CompoundContainer sectionNBT)
		{
			return sectionNBT.Get<long[]>("BlockStates");
		}

		protected virtual string AppendBlockStateBitsToBitStream(string bitStream, string newBits, int indexBitCount)
		{
			bitStream += newBits;
			return bitStream;
		}

		public override void WriteBiomes(ChunkData c, CompoundContainer chunkNBT)
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

		public override void LoadBiomes(ChunkData c, CompoundContainer chunkNBT)
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

		public override void LoadTileTicks(ChunkData c, CompoundContainer chunkNBT)
		{
			//TODO
		}

		public override void WriteTileTicks(ChunkData c, CompoundContainer chunkNBT)
		{
			//Add "post processing" positions (i.e. block positions that need an update)
			var ppList = chunkNBT.Add("PostProcessing", new ListContainer(NBTTag.TAG_List));
			for (int i = 0; i < 16; i++)
			{
				ppList.Add(new ListContainer(NBTTag.TAG_Short));
			}

			foreach (var t in c.postProcessTicks)
			{
				int listIndex = t.y / 16;
				var x = t.x % 16;
				var y = t.y % 16;
				var z = t.z % 16;
				var list = ppList.Get<ListContainer>(listIndex);
				short packed = (short)((z << 8) + (y << 4) + x);
				list.Add(packed);
			}
		}
	}
}

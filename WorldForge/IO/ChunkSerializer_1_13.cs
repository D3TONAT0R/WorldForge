using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;
using static System.Collections.Specialized.BitVector32;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_13 : ChunkSerializerAnvil
	{
		public ChunkSerializer_1_13(GameVersion version) : base(version) { }

		public virtual bool UseFull64BitRange => true;

		public override void WriteCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			base.WriteCommonData(c, chunkNBT);

			//TODO: find out in which version these tags were added
			chunkNBT.Add("Status", c.Status.ToString());
			chunkNBT.Add("InhabitedTime", c.InhabitedTime);

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
				paletteList.Add(block.ToNBT());
			}
			comp.Add(PaletteKey, paletteList);

			int bitsPerBlock = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
			comp.Add(BlockStatesKey, BitUtils.PackBits(GetBlockIndexArray(section), bitsPerBlock, UseFull64BitRange));
		}

		public override void LoadCommonData(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			if(chunkNBT.TryGet("Status", out string statusString))
			{
				statusString = statusString.Replace("minecraft:", "");
				if(Enum.TryParse(statusString, out ChunkStatus s))
				{
					c.Status = s;
				}
			}
			else if(chunkNBT.TryGet("status", out statusString))
			{
				if(Enum.TryParse(statusString, out ChunkStatus s))
				{
					c.Status = s;
				}
			}
		}

		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound, GameVersion? version)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach(var o in sectionsList.listContent)
			{
				var section = new ChunkSection(c, null);

				var sectionComp = (NBTCompound)o;
				if(!SectionHasBlocks(sectionComp)) continue;
				sbyte secY = ParseSectionYIndex(sectionComp);

				section.palette.Clear();
				foreach(var cont in GetBlockPalette(sectionComp).listContent)
				{
					var paletteItem = (NBTCompound)cont;
					section.palette.Add(new BlockState(paletteItem));
				}

				if(section.palette.Count == 1)
				{
					//Do nothing, as all blocks already have an index of 0
				}
				else
				{
					//1.15 and prior uses the full range of bits where 1.16 and later doesn't use the last bits if they can't contain a block index
					int indexBitCount = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
					long[] longs = GetBlockDataArray(sectionComp);

					ushort[] indices = BitUtils.ExtractCompressedInts(longs, indexBitCount, 4096, UseFull64BitRange);

					for(int y = 0; y < 16; y++)
					{
						for(int z = 0; z < 16; z++)
						{
							for(int x = 0; x < 16; x++)
							{
								section.blocks[ChunkSection.GetIndex(x, y, z)] = indices[y * 256 + z * 16 + x];
							}
						}
					}
				}
				c.Sections.Add(secY, section);
			}
		}

		protected virtual NBTList GetBlockPalette(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsList("Palette");
		}

		protected virtual long[] GetBlockDataArray(NBTCompound sectionNBT)
		{
			return sectionNBT.Get<long[]>("BlockStates");
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			if(chunkNBT.TryGet<int[]>("Biomes", out var biomeData))
			{
				for(int y = 0; y < 64; y++)
				{
					for(int x = 0; x < 4; x++)
					{
						for(int z = 0; z < 4; z++)
						{
							int i = y * 16 + z * 4 + x;
							var biome = (BiomeID)biomeData[i];
							var section = c.GetChunkSectionForYCoord(y * 4, false);
							if(section != null)
							{
								section.SetBiome3D4x4At(x * 4, y * 4, z * 4, biome);
							}
						}
					}
				}
			}
		}

		public override void LoadTileTicks(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			//TODO
		}

		public override void WriteTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//Add "post processing" positions (i.e. block positions that need an update)
			var ppList = chunkNBT.Add("PostProcessing", new NBTList(NBTTag.TAG_List));
			for(int i = 0; i < 16; i++)
			{
				ppList.Add(new NBTList(NBTTag.TAG_Short));
			}

			foreach(var t in c.PostProcessTicks)
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

		protected ushort[] GetBlockIndexArray(ChunkSection section)
		{
			ushort[] blockData = new ushort[16 * 16 * 16];
			for(int y = 0; y < 16; y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x++)
					{
						blockData[y * 256 + z * 16 + x] = section.blocks[ChunkSection.GetIndex(x, y, z)];
					}
				}
			}
			return blockData;
		}
	}
}

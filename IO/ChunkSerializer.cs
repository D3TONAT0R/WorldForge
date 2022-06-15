using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public static class ChunkSerializer
	{
		public static NBTContent CreateCompoundForChunk(ChunkData chunk, Version version)
		{
			var nbt = new NBTContent();
			nbt.dataVersion = version.GetDataVersion();
			nbt.contents.Add("xPos", chunk.coords.x);
			nbt.contents.Add("zPos", chunk.coords.z);
			nbt.contents.Add("Status", "light");
			ListContainer sections = new ListContainer(NBTTag.TAG_Compound);
			nbt.contents.Add("Sections", sections);
			nbt.contents.Add("TileEntities", new ListContainer(NBTTag.TAG_Compound));
			nbt.contents.Add("Entities", new ListContainer(NBTTag.TAG_Compound));

			//Add the rest of the tags and leave them empty
			nbt.contents.Add("Heightmaps", new CompoundContainer());
			nbt.contents.Add("Structures", new CompoundContainer());
			nbt.contents.Add("LiquidTicks", new ListContainer(NBTTag.TAG_Compound));
			nbt.contents.Add("TileTicks", new ListContainer(NBTTag.TAG_Compound));

			//Add post processing lists
			var ppList = new ListContainer(NBTTag.TAG_List);
			for (int i = 0; i < 16; i++)
			{
				ppList.Add(new ListContainer(NBTTag.TAG_Short));
			}
			nbt.contents.Add("PostProcessing", ppList);

			//Write the actual data
			chunk.WriteToNBT(nbt.contents, version);

			/*
			ListContainer postprocessing = new ListContainer(NBTTag.TAG_List);
			for(int i = 0; i < 16; i++) postprocessing.Add("", new ListContainer(NBTTag.TAG_List));
			nbt.contents.Add("PostProcessing", postprocessing);
			nbt.contents.Add("InhabitedTime", 0L);
			nbt.contents.Add("LastUpdate", 0L);
			*/
			return nbt;
		}

		///<summary>Reads all blocks from the given chunk NBT</summary>
		public static void ReadBlocksFromNBT(ChunkData chunk, int? dataVersion)
		{
			Version? version = Version.FromDataVersion(dataVersion);
			var nbtCompound = chunk.sourceNBT.contents;
			if (nbtCompound.Contains("Sections") || nbtCompound.Contains("sections"))
			{
				LoadBlocksAnvilFormat(chunk, nbtCompound, version);
			}
			else
			{
				LoadBlocksMCRFormat(chunk, nbtCompound);
			}
		}

		static void LoadBlocksAnvilFormat(ChunkData chunk, CompoundContainer nbtCompound, Version? version)
		{
			//TODO: find out when format has changed, somewhere between 1.16 and 1.19
			if (version < Version.Release_1(18))
			{
				var sectionsList = nbtCompound.GetAsList("Sections");
				foreach (var o in sectionsList.cont)
				{
					var section = new ChunkSection(null);

					var compound = (CompoundContainer)o;
					if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
					sbyte secY;
					unchecked
					{
						//secY = Convert.ToSByte(compound.Get("Y"));
						secY = (sbyte)compound.Get<byte>("Y");
					}
					section.palette.Clear();
					foreach (var cont in compound.GetAsList("Palette").cont)
					{
						CompoundContainer block = (CompoundContainer)cont;
						//HACK: bypass error throw
						var proto = BlockList.Find((string)block.Get("Name"), true);
						if (proto != null)
						{
							var bs = new BlockState(proto);
							if (block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
							section.palette.Add(bs);
						}
						else
						{
							section.palette.Add(BlockState.Unknown);
						}
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
						if (version == null || version.Value < Version.Release_1(16, 0))
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
					chunk.sections.Add(secY, section);
				}
			}
			else
			{
				//TODO: needs refactoring, too much duplicated code
				var sectionsList = nbtCompound.GetAsList("sections");
				foreach (var o in sectionsList.cont)
				{
					var section = new ChunkSection(null);

					var compound = (CompoundContainer)o;
					if (!compound.Contains("Y") || !compound.Contains("block_states") || !compound.GetAsCompound("block_states").Contains("data")) continue;
					sbyte secY;
					unchecked
					{
						secY = (sbyte)compound.Get<byte>("Y");
						//secY = Convert.ToSByte(compound.Get("Y"));
					}
					var blockStates = compound.GetAsCompound("block_states");
					section.palette.Clear();
					foreach (var cont in blockStates.GetAsList("palette").cont)
					{
						CompoundContainer block = (CompoundContainer)cont;
						//HACK: bypass error throw
						var proto = BlockList.Find((string)block.Get("Name"), true);
						if (proto != null)
						{
							var bs = new BlockState(proto);
							if (block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
							section.palette.Add(bs);
						}
						else
						{
							section.palette.Add(BlockState.Unknown);
						}
					}

					//1.15 uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
					int indexLength = Math.Max(4, (int)Math.Log(section.palette.Count - 1, 2.0) + 1);
					long[] longs = blockStates.Get<long[]>("data");
					string bits = "";
					for (int i = 0; i < longs.Length; i++)
					{
						string newBits = "";
						byte[] bytes = BitConverter.GetBytes(longs[i]);
						for (int j = 0; j < 8; j++)
						{
							newBits += Converter.ByteToBinary(bytes[j], true);
						}
						if (version == null || version.Value < Version.Release_1(16, 0))
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
					chunk.sections.Add(secY, section);
				}
			}
		}

		static void LoadBlocksMCRFormat(ChunkData chunk, CompoundContainer nbtCompound)
		{
			byte[] blocks = nbtCompound.Get<byte[]>("Blocks");
			byte[] add; //TODO: include "Add" bits in ID (from modded worlds)
			if(nbtCompound.Contains("Add")) add = nbtCompound.Get<byte[]>("Add");
			byte[] meta = nbtCompound.Get<byte[]>("Data"); //TODO: also include meta in block lookup
			for(int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for(int y = 0; y < 128; y++)
					{
						byte id = blocks[(x * 16 + z) * 128 + y];
						var block = BlockList.FindByNumeric(new NumericID(id, 0));
						if(block != null)
						{
							chunk.SetBlockAt(x, y, z, new BlockState(block));
						}
					}
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using MCUtils;
using MCUtils.Coordinates;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class ChunkSerializerAnvil : ChunkSerializer
	{
		public ChunkSerializerAnvil(Version version) : base(version) { }

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

				ParseNumericIDBlocks(c, nbtCompound);

				c.sections.Add(secY, section);
			}
		}

		protected void ParseNumericIDBlocks(ChunkData c, CompoundContainer nbtCompound)
		{
			byte[] blocks = nbtCompound.Get<byte[]>("Blocks");
			byte[] add; //TODO: include "Add" bits in ID (from modded worlds)
			if (nbtCompound.Contains("Add")) add = nbtCompound.Get<byte[]>("Add");
			byte[] meta = nbtCompound.Get<byte[]>("Data"); //TODO: also include meta in block lookup
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 128; y++)
					{
						byte id = blocks[(x * 16 + z) * 128 + y];
						var block = BlockList.FindByNumeric(new NumericID(id, 0));
						if (block != null)
						{
							c.SetBlockAt(x, y, z, new BlockState(block));
						}
					}
				}
			}
		}

		protected virtual ListContainer GetSectionsList(CompoundContainer chunkNBT)
		{
			return chunkNBT.GetAsList("Sections");
		}

		protected virtual sbyte ParseSectionYIndex(CompoundContainer sectionNBT)
		{
			unchecked
			{
				return (sbyte)sectionNBT.Get<byte>("Y");
			}
		}

		protected virtual bool HasBlocks(CompoundContainer sectionNBT)
		{
			return sectionNBT.Contains("Blocks");
		}

		public override void LoadTileEntities(ChunkData c, CompoundContainer chunkNBT)
		{
			c.tileEntities = new Dictionary<Coordinates.BlockCoord, TileEntity>();
			if (chunkNBT.Contains("TileEntities"))
			{
				var tileEntList = chunkNBT.GetAsList("TileEntities");
				if (tileEntList != null && tileEntList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < tileEntList.Length; i++)
					{
						var te = new TileEntity(tileEntList.Get<CompoundContainer>(i));
						c.tileEntities.Add(new BlockCoord(te.BlockPosX, te.BlockPosY, te.BlockPosZ), te);
					}
				}
			}
		}

		public override void LoadEntities(ChunkData c, CompoundContainer chunkNBT, Region parentRegion)
		{
			c.entities = new List<Entity>();
			if (chunkNBT.Contains("Entities"))
			{
				var entList = chunkNBT.GetAsList("Entities");
				if (entList != null && entList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < entList.Length; i++)
					{
						c.entities.Add(new Entity(entList.Get<CompoundContainer>(i)));
					}
				}
			}
		}

		//TODO: load 3D biomes from 1.18 (?) and renamed biome structure in 1.19 (?)
		public override void LoadBiomes(ChunkData c, CompoundContainer chunkNBT)
		{
			if (chunkNBT.Contains("Biomes"))
			{
				var biomeArray = chunkNBT.Get<int[]>("Biomes");
				//Read topmost section, to avoid future cave biomes underground
				int offset = biomeArray.Length - 16;
				for (int x = 0; x < 4; x++)
				{
					for (int z = 0; z < 4; z++)
					{
						var value = (byte)biomeArray[offset + z * 4 + x];
						for (int x1 = 0; x1 < 4; x1++)
						{
							for (int z1 = 0; z1 < 4; z1++)
							{
								c.biomes[x * 4 + x1, z * 4 + z1] = (BiomeID)value;
							}
						}
					}
				}
			}
			else
			{
				//Default to plains biome
				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						c.biomes[x, z] = BiomeID.plains;
					}
				}
			}
		}

		public override void LoadTicks(ChunkData c, CompoundContainer chunkNBT)
		{
			//TODO
		}
	}
}

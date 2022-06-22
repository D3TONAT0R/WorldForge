using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCUtils;
using MCUtils.Coordinates;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class ChunkSerializerAnvil : ChunkSerializer
	{
		public ChunkSerializerAnvil(Version version) : base(version) { }

		#region Blocks
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
			byte[] compressedMetaNibbles = nbtCompound.Get<byte[]>("Data");
			byte[] meta = new byte[compressedMetaNibbles.Length * 2];
			for (int i = 0; i < compressedMetaNibbles.Length; i++)
			{
				BitUtils.ExtractNibbles(compressedMetaNibbles[i], out var low, out var high);
				meta[i * 2] = low;
				meta[i * 2 + 1] = high;
			}
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 128; y++)
					{
						int i = (x * 16 + z) * 128 + y;
						var block = BlockList.FindByNumeric(new NumericID(blocks[i], meta[i]));
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

		public override void LoadCommonData(ChunkData c, CompoundContainer chunkNBT)
		{
			
		}

		public override void WriteCommonData(ChunkData c, CompoundContainer chunkNBT)
		{
			chunkNBT.Add("xPos", c.coords.x);
			chunkNBT.Add("zPos", c.coords.z);
			chunkNBT.Add("Status", "light");
			chunkNBT.Add("InhabitedTime", c.inhabitedTime);

			//Leave it empty (or implement heightmap gen in the future?)
			chunkNBT.Add("Heightmaps", new CompoundContainer());

			//TODO: find out in which version these tags were added
			chunkNBT.Add("Structures", new CompoundContainer());
		}

		public override void WriteBlocks(ChunkData c, CompoundContainer chunkNBT)
		{
			var sectionList = chunkNBT.Add("Sections", new ListContainer(NBTTag.TAG_Compound));
			for (sbyte secY = c.LowestSection; secY < c.HighestSection; secY++)
			{
				if (c.sections.TryGetValue(secY, out var section))
				{
					var sectionNBT = sectionList.Add(new CompoundContainer());
					unchecked
					{
						sectionNBT.Add("Y", (byte)secY);
					}
					WriteSection(section, sectionNBT, secY);
				}
			}
		}

		public virtual void WriteSection(ChunkSection c, CompoundContainer sectionNBT, sbyte sectionY)
		{
			byte[] ids = new byte[4096];
			byte[] metaNibbles = new byte[4096];
			for (int y = 0; y < 16; y++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int x = 0; x < 16; x++)
					{
						int i = (x * 16 + z) + 128 * y;
						if (BlockList.numerics.TryGetValue(c.GetBlockAt(x, y, z).block, out var numID))
						{
							ids[i] = numID.id;
							metaNibbles[i] = numID.meta;
						}
					}
				}
			}
			byte[] compressedMetaNibbles = new byte[2048];
			for (int i = 0; i < 2048; i++)
			{
				compressedMetaNibbles[i] = BitUtils.CompressNibbles(metaNibbles[i * 2], metaNibbles[i * 2 + 1]);
			}
			//Do not write SkyLight and BlockLight (byte[2048]), let them generate by the game
		}

		#endregion

		//TODO: which game version is this for?
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

		//TODO: which game version is this for?
		public override void WriteTileEntities(ChunkData c, CompoundContainer chunkNBT)
		{
			var comp = chunkNBT.AddList("TileEntities", NBTTag.TAG_Compound);
			foreach(var te in c.tileEntities.Values)
			{
				comp.Add(te.NBTCompound);
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

		public override void WriteEntities(ChunkData c, CompoundContainer chunkNBT, Region parentRegion)
		{

		}

		public override void LoadBiomes(ChunkData c, CompoundContainer chunkNBT)
		{
			if (chunkNBT.TryGet<byte[]>("Biomes", out var biomeData))
			{
				byte defaultBiomeID = (byte)BiomeID.plains;
				if(biomeData.All(b => b == defaultBiomeID))
				{
					//Do nothing, as all biomes are plains biomes anyway (the default biome)
				}
				else
				{
					for (int i = 0; i < 256; i++)
					{
						c.SetBiomeAt(i % 16, i / 16, (BiomeID)biomeData[i]);
					}
				}
			}
		}

		public override void WriteBiomes(ChunkData c, CompoundContainer chunkNBT)
		{
			byte[] biomeData = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				biomeData[i] = (byte)c.GetBiomeAt(i % 16, i / 16);
			}
			chunkNBT.Add("Biomes", biomeData);
		}

		public override void LoadTileTicks(ChunkData c, CompoundContainer chunkNBT)
		{
			//TODO
		}

		public override void WriteTileTicks(ChunkData c, CompoundContainer chunkNBT)
		{
			var tickList = chunkNBT.Add("TileTicks", new ListContainer(NBTTag.TAG_Compound));
			foreach(var t in c.postProcessTicks)
			{
				CompoundContainer tick = new CompoundContainer();
				tick.Add("i", BlockList.numerics[c.GetBlockAt(t.x, t.y, t.z).block]);
				tick.Add("p", 0);
				tick.Add("t", 0);
				tick.Add("x", t.x);
				tick.Add("y", t.y);
				tick.Add("z", t.z);
				tickList.Add(tick);
			}
		}
	}
}

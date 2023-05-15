using MCUtils.Coordinates;
using MCUtils.NBT;
using MCUtils.TileEntities;
using System.Collections.Generic;
using System.Linq;

namespace MCUtils.IO
{
	public class ChunkSerializerAnvil : ChunkSerializer
	{
		public ChunkSerializerAnvil(Version version) : base(version) { }

		public int GetIndex(int x, int y, int z)
		{
			return (z * 16 + x) + 256 * y;
		}

		#region Blocks
		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach (var o in sectionsList.listContent)
			{
				var sectionComp = (NBTCompound)o;
				if (!HasBlocks(sectionComp)) continue;
				sbyte secY = ParseSectionYIndex(sectionComp);
				ParseNumericIDBlocks(c, sectionComp, secY);
			}
		}

		protected void ParseNumericIDBlocks(ChunkData c, NBTCompound nbtCompound, sbyte sectionY)
		{
			byte[] blocks = nbtCompound.Get<byte[]>("Blocks");
			byte[] add; //TODO: include "Add" bits in ID (from modded worlds)
			if (nbtCompound.Contains("Add")) add = nbtCompound.Get<byte[]>("Add");
			byte[] meta = BitUtils.ExtractNibblesFromByteArray(nbtCompound.Get<byte[]>("Data"));
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 16; y++)
					{
						int i = GetIndex(x,y,z);
						var block = BlockList.FindByNumeric(new NumericID(blocks[i], meta[i]));
						if (block != null)
						{
							c.SetBlockAt(x, y + sectionY * 16, z, new BlockState(block));
						}
					}
				}
			}
		}

		protected virtual NBTList GetSectionsList(NBTCompound chunkNBT)
		{
			return chunkNBT.GetAsList("Sections");
		}

		protected virtual sbyte ParseSectionYIndex(NBTCompound sectionNBT)
		{
			unchecked
			{
				return (sbyte)sectionNBT.Get<byte>("Y");
			}
		}

		protected virtual bool HasBlocks(NBTCompound sectionNBT)
		{
			return sectionNBT.Contains("Blocks");
		}

		public override void LoadCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			
		}

		public override void WriteCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			chunkNBT.Add("xPos", c.worldSpaceCoord.x);
			chunkNBT.Add("zPos", c.worldSpaceCoord.z);
			chunkNBT.Add("TerrainPopulated", (byte)1);
			chunkNBT.Add("LastUpdate", 0L);
			//TODO: create proper height map
			int[] hm = new int[256];
			for(int z = 0; z < 16; z++)
			{
				for(int x = 0; x < 16; x++)
				{
					hm[z * 16 + x] = c.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
				}
			}
			chunkNBT.Add("HeightMap", hm);
		}

		public override void WriteBlocks(ChunkData c, NBTCompound chunkNBT)
		{
			var sectionList = chunkNBT.Add("Sections", new NBTList(NBTTag.TAG_Compound));
			for (sbyte secY = c.LowestSection; secY <= c.HighestSection; secY++)
			{
				if (c.sections.TryGetValue(secY, out var section))
				{
					var sectionNBT = sectionList.Add(new NBTCompound());
					unchecked
					{
						sectionNBT.Add("Y", (byte)secY);
					}
					WriteSection(section, sectionNBT, secY);
				}
			}
		}

		public virtual void WriteSection(ChunkSection c, NBTCompound sectionNBT, sbyte sectionY)
		{
			byte[] ids = new byte[4096];
			byte[] metaNibbles = new byte[4096];
			for (int y = 0; y < 16; y++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int x = 0; x < 16; x++)
					{
						int i = GetIndex(x, y, z);
						if (BlockList.numerics.TryGetValue(c.GetBlockAt(x, y, z).block, out var numID))
						{
							ids[i] = numID.id;
							metaNibbles[i] = numID.meta;
						}
					}
				}
			}
			sectionNBT.Add("Blocks", ids);
			sectionNBT.Add("Data", BitUtils.CompressNibbleArray(metaNibbles));

			sectionNBT.Add("BlockLight", new byte[2048]);
			byte[] sl = new byte[2048];
			for(int i = 0; i < 2048; i++) sl[i] = 255;
			sectionNBT.Add("SkyLight", new byte[2048]);
			//Do not write SkyLight and BlockLight (byte[2048]), let them generate by the game
		}

		#endregion

		//TODO: which game version is this for?
		public override void LoadTileEntities(ChunkData c, NBTCompound chunkNBT)
		{
			c.tileEntities = new Dictionary<Coordinates.BlockCoord, TileEntity>();
			if (chunkNBT.Contains("TileEntities"))
			{
				var tileEntList = chunkNBT.GetAsList("TileEntities");
				if (tileEntList != null && tileEntList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < tileEntList.Length; i++)
					{
						var te = TileEntity.CreateFromNBT(tileEntList.Get<NBTCompound>(i));
						c.tileEntities.Add(te.blockPos, te);
					}
				}
			}
		}

		//TODO: which game version is this for?
		public override void WriteTileEntities(ChunkData c, NBTCompound chunkNBT)
		{
			var comp = chunkNBT.AddList("TileEntities", NBTTag.TAG_Compound);
			foreach(var te in c.tileEntities.Values)
			{
				comp.Add(te.ToNBT(TargetVersion));
			}
		}

		public override void LoadEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion)
		{
			c.entities = new List<Entity>();
			if (chunkNBT.Contains("Entities"))
			{
				var entList = chunkNBT.GetAsList("Entities");
				if (entList != null && entList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < entList.Length; i++)
					{
						c.entities.Add(new Entity(entList.Get<NBTCompound>(i)));
					}
				}
			}
		}

		public override void WriteEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion)
		{
			chunkNBT.AddList("Entities", NBTTag.TAG_Compound);
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT)
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

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			byte[] biomeData = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				biomeData[i] = (byte)c.GetBiomeAt(i % 16, i / 16);
			}
			chunkNBT.Add("Biomes", biomeData);
		}

		public override void LoadTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//TODO
		}

		public override void WriteTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			var tickList = chunkNBT.Add("TileTicks", new NBTList(NBTTag.TAG_Compound));
			foreach(var t in c.postProcessTicks)
			{
				NBTCompound tick = new NBTCompound();
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

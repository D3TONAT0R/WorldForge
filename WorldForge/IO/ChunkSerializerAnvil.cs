using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.Entities;
using WorldForge.NBT;
using WorldForge.Regions;
using WorldForge.TileEntities;

namespace WorldForge.IO
{
	public class ChunkSerializerAnvil : ChunkSerializer
	{

		public virtual string SectionsCompName => "Sections";
		public virtual string TileEntitiesCompName => "TileEntities";
		public virtual string EntitiesCompName => "Entities";
		public virtual string BiomesCompName => "Biomes";

		public virtual string BlocksCompName => "Blocks";
		public virtual string BlockDataCompName => "Data";
		public virtual string BlockPaletteCompName => "Palette";
		public virtual string BlockLightCompName => "BlockLight";
		public virtual string SkyLightCompName => "SkyLight";

		public ChunkSerializerAnvil(GameVersion version) : base(version) { }

		public int GetIndex(int x, int y, int z)
		{
			return (z * 16 + x) + 256 * y;
		}

		#region Blocks
		public override void LoadBlocks(Chunk c, NBTCompound nbtCompound, GameVersion? version)
		{
			var sectionsList = GetSectionsList(nbtCompound);
			foreach(var o in sectionsList.listContent)
			{
				var sectionComp = (NBTCompound)o;
				if(!SectionHasBlocks(sectionComp)) continue;
				sbyte secY = ParseSectionYIndex(sectionComp);
				ParseNumericIDBlocks(c, sectionComp, secY);
			}
		}

		protected void ParseNumericIDBlocks(Chunk c, NBTCompound nbtCompound, sbyte sectionY)
		{
			byte[] blocks = nbtCompound.Get<byte[]>(BlocksCompName);

			byte[] add; //TODO: include "Add" bits in ID (from modded worlds)
			if(nbtCompound.Contains("Add")) add = nbtCompound.Get<byte[]>("Add");

			byte[] meta = BitUtils.ExtractNibblesFromByteArray(nbtCompound.Get<byte[]>(BlockDataCompName));
			for(int x = 0; x < 16; x++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int y = 0; y < 16; y++)
					{
						int i = GetIndex(x, y, z);
						var block = BlockState.FromNumericID(new NumericID(blocks[i], meta[i]));
						if(block != null)
						{
							c.SetBlock((x, y + sectionY * 16, z), block);
						}
					}
				}
			}
		}

		protected NBTList GetSectionsList(NBTCompound chunkNBT)
		{
			return chunkNBT.GetAsList(SectionsCompName);
		}

		protected virtual sbyte ParseSectionYIndex(NBTCompound sectionNBT)
		{
			unchecked
			{
				return (sbyte)sectionNBT.Get<byte>("Y");
			}
		}

		protected virtual bool SectionHasBlocks(NBTCompound sectionNBT)
		{
			return sectionNBT.Contains(BlocksCompName);
		}

		public override void LoadCommonData(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{

		}

		public override void WriteCommonData(Chunk c, NBTCompound chunkNBT)
		{
			chunkNBT.Add("xPos", c.WorldSpaceCoord.x);
			chunkNBT.Add("zPos", c.WorldSpaceCoord.z);
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

		public override void WriteBlocks(Chunk c, NBTCompound chunkNBT)
		{
			var sectionList = chunkNBT.Add(SectionsCompName, new NBTList(NBTTag.TAG_Compound));
			for(sbyte secY = c.LowestSection; secY <= c.HighestSection; secY++)
			{
				if(c.Sections.TryGetValue(secY, out var section))
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
			for(int y = 0; y < 16; y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x++)
					{
						int i = GetIndex(x, y, z);
						var blockState = c.GetBlock(x, y, z);
						BlockState.ResolveBlockState(TargetVersion, ref blockState);
						if(blockState != null)
						{
							var numID = blockState.ToNumericID(TargetVersion) ?? NumericID.Air;
							if(numID.id < 0 || numID.id > 255) throw new IndexOutOfRangeException("Block ID out of range (0-255)");
							if(numID.damage < 0 || numID.damage > 15) throw new IndexOutOfRangeException("Block meta exceeds limit of (0-15)");
							ids[i] = (byte)numID.id;
							metaNibbles[i] = (byte)numID.damage;
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
		public override void LoadTileEntities(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{
			c.TileEntities.Clear();
			if(chunkNBT.Contains(TileEntitiesCompName))
			{
				var tileEntList = chunkNBT.GetAsList(TileEntitiesCompName);
				if(tileEntList != null && tileEntList.ContentsType == NBTTag.TAG_Compound)
				{
					for(int i = 0; i < tileEntList.Length; i++)
					{
						var te = TileEntity.CreateFromNBT(tileEntList.Get<NBTCompound>(i), version, out var blockPos);
						c.TileEntities.Add(blockPos, te);
					}
				}
			}
		}

		public override void WriteTileEntities(Chunk c, NBTCompound chunkNBT)
		{
			var comp = chunkNBT.AddList(TileEntitiesCompName, NBTTag.TAG_Compound);
			foreach(var k in c.TileEntities.Keys)
			{
				var tileEntity = c.TileEntities[k];
				var nbt = tileEntity.ToNBT(TargetVersion, k);
				if(nbt != null)
				{
					comp.Add(nbt);
				}
			}
		}

		public override void LoadEntities(Chunk c, NBTCompound chunkNBT, Region parentRegion, GameVersion? version)
		{
			c.Entities.Clear();
			if(chunkNBT.Contains("Entities"))
			{
				var entList = chunkNBT.GetAsList("Entities");
				if(entList != null && entList.ContentsType == NBTTag.TAG_Compound)
				{
					for(int i = 0; i < entList.Length; i++)
					{
						c.Entities.Add(Entity.CreateFromNBT(entList.Get<NBTCompound>(i)));
					}
				}
			}
		}

		public override void WriteEntities(Chunk c, NBTCompound chunkNBT)
		{
			var list = chunkNBT.AddList("Entities", NBTTag.TAG_Compound);
			foreach(var e in c.Entities)
			{
				//TODO: serialize entities
				//list.Add(e.ToNBT(TargetVersion));
			}
		}

		public override void LoadBiomes(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{
			if(chunkNBT.TryGet<byte[]>("Biomes", out var biomeData))
			{
				for(int i = 0; i < 256; i++)
				{
					c.SetBiomeAt(i % 16, i / 16, BiomeIDs.GetFromNumeric(biomeData[i]));
				}
			}
		}

		public override void WriteBiomes(Chunk c, NBTCompound chunkNBT)
		{
			byte[] biomeData = new byte[256];
			for(int i = 0; i < 256; i++)
			{
				var biome = c.GetBiomeAt(i % 16, i / 16) ?? c.ParentDimension?.DefaultBiome ?? BiomeID.Plains;
				biomeData[i] = biome.ResolveNumericIDForVersion(TargetVersion);
			}
			chunkNBT.Add("Biomes", biomeData);
		}

		public override void LoadTileTicks(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{
			//TODO
		}

		public override void WriteTileTicks(Chunk c, NBTCompound chunkNBT)
		{
			var tickList = chunkNBT.Add("TileTicks", new NBTList(NBTTag.TAG_Compound));
			foreach(var t in c.PostProcessTicks)
			{
				NBTCompound tick = new NBTCompound();
				var numID = c.GetBlock(t).Block.numericID;
				tick.Add("i", numID.HasValue ? (byte)numID.Value.id : 0);
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

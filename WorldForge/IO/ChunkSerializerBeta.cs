using System;
using WorldForge.Chunks;
using WorldForge.Entities;
using WorldForge.NBT;
using WorldForge.Regions;
using WorldForge.TileEntities;

namespace WorldForge.IO
{
	public class ChunkSerializerBeta : ChunkSerializer
	{
		public ChunkSerializerBeta(GameVersion version) : base(version) { }

		public override void LoadCommonData(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{

		}

		public override void WriteCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			chunkNBT.Add("xPos", c.WorldSpaceCoord.x);
			chunkNBT.Add("zPos", c.WorldSpaceCoord.z);
			chunkNBT.Add("TerrainPopulated", (byte)1);
			chunkNBT.Add("LastUpdate", 0L);
			//TODO: Light data must be generated 
			var sl = new byte[16384];
			for(int i = 0; i < 16384; i++)
			{
				sl[i] = 255;
			}
			chunkNBT.Add("SkyLight", sl);
			chunkNBT.Add("BlockLight", new byte[16384]);
		}

		#region Blocks

		protected int GetArrayIndex(int x, int y, int z)
		{
			return x * 2048 + z * 128 + y;
		}

		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound, GameVersion? version)
		{
			if(nbtCompound.TryGet<byte[]>("Blocks", out var blocks))
			{
				byte[] meta = BitUtils.ExtractNibblesFromByteArray(nbtCompound.Get<byte[]>("Data"));
				for(int y = 0; y < 128; y++)
				{
					for(int z = 0; z < 16; z++)
					{
						for(int x = 0; x < 16; x++)
						{
							int i = GetArrayIndex(x, y, z);
							var block = BlockList.FindByNumeric(new NumericID(blocks[i], meta[i]));
							if(block != null)
							{
								c.SetBlockAt((x, y, z), new BlockState(block));
							}
						}
					}
				}
			}
		}

		public override void WriteBlocks(ChunkData c, NBTCompound chunkNBT)
		{
			byte[] blocks = new byte[32768];
			byte[] metaNibbles = new byte[32768];
			for(int y = 0; y < 128; y++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int x = 0; x < 16; x++)
					{
						var blockState = c.GetBlockAt((x, y, z));
						BlockState.ResolveBlockState(TargetVersion, ref blockState); //Resolve block state (substitute blocks if necessary)
						if(blockState != null)
						{
							var numID = blockState.ToNumericID(TargetVersion) ?? NumericID.Air;
							var i = GetArrayIndex(x, y, z);
							if(numID.id < 0 || numID.id > 255) throw new IndexOutOfRangeException("Block ID out of range (0-255)");
							if(numID.damage < 0 || numID.damage > 15) throw new IndexOutOfRangeException("Block meta exceeds limit of (0-15)");
							blocks[i] = (byte)numID.id;
							metaNibbles[i] = (byte)numID.damage;
						}
					}
				}
			}
			chunkNBT.Add("Blocks", blocks);
			chunkNBT.Add("Data", BitUtils.CompressNibbleArray(metaNibbles));
		}

		#endregion

		//TODO: which game version is this for?
		public override void LoadTileEntities(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			c.TileEntities.Clear();
			if(chunkNBT.Contains("TileEntities"))
			{
				var tileEntList = chunkNBT.GetAsList("TileEntities");
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

		public override void WriteTileEntities(ChunkData c, NBTCompound chunkNBT)
		{
			var comp = chunkNBT.AddList("TileEntities", NBTTag.TAG_Compound);
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

		public override void LoadEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion, GameVersion? version)
		{
			c.Entities.Clear();
			if(chunkNBT.TryGet<NBTList>("Entities", out var entList))
			{
				if(entList.ContentsType == NBTTag.TAG_Compound)
				{
					for(int i = 0; i < entList.Length; i++)
					{
						c.Entities.Add(Entity.CreateFromNBT(entList.Get<NBTCompound>(i)));
					}
				}
			}
		}

		protected override void PostWrite(ChunkData c, NBTCompound chunkNBT)
		{
			var hm = new byte[256];
			for(int z = 0; z < 16; z++)
			{
				for(int x = 0; x < 16; x++)
				{
					hm[z * 16 + x] = (byte)c.GetHighestBlock(x, z);
				}
			}
			chunkNBT.Add("HeightMap", hm);
		}

		public override void WriteEntities(ChunkData c, NBTCompound chunkNBT)
		{
			chunkNBT.Add("Entities", new NBTList(NBTTag.TAG_Compound));
			//TODO
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			//Do nothing, saved biomes were not implemented back then, they were generated on the fly by the game (based on the world seed)
		}

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			//Do nothing, saved biomes were not implemented back then, they were generated on the fly by the game (based on the world seed)
		}

		public override void LoadTileTicks(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			//Do nothing, tile ticks were not implemented in old regions.
		}

		public override void WriteTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//Do nothing, tile ticks were not implemented in old regions.
		}
	}
}

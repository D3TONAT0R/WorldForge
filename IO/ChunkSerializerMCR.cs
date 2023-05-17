using MCUtils.Coordinates;
using MCUtils.NBT;
using MCUtils.TileEntities;
using System.Collections.Generic;

namespace MCUtils.IO
{
	public class ChunkSerializerMCR : ChunkSerializer
	{
		public ChunkSerializerMCR(Version version) : base(version) { }

		public override void LoadCommonData(ChunkData c, NBTCompound chunkNBT, Version? version)
		{

		}

		public override void WriteCommonData(ChunkData c, NBTCompound chunkNBT)
		{
			chunkNBT.Add("xPos", c.worldSpaceCoord.x);
			chunkNBT.Add("zPos", c.worldSpaceCoord.z);
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

		public override void LoadBlocks(ChunkData c, NBTCompound nbtCompound, Version? version)
		{
			if(nbtCompound.TryGet<byte[]>("Blocks", out var blocks))
			{
				byte[] meta = BitUtils.ExtractNibblesFromByteArray(nbtCompound.Get<byte[]>("Data"));
				for(int y = 0; y < 128; y++)
				{
					for(int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x++)
						{
							int i = GetArrayIndex(x, y, z);
							var block = BlockList.FindByNumeric(new NumericID(blocks[i], meta[i]));
							if (block != null)
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
				for (int z = 0; z < 16; z++)
				{
					for (int x = 0; x < 16; x++)
					{
						var blockState = c.GetBlockAt((x, y, z));
						if(blockState != null && BlockList.numerics.TryGetValue(blockState.block, out var numID))
						{
							var i = GetArrayIndex(x, y, z);
							blocks[i] = numID.id;
							metaNibbles[i] = numID.meta;
						}
					}
				}
			}
			chunkNBT.Add("Blocks", blocks);
			chunkNBT.Add("Data", BitUtils.CompressNibbleArray(metaNibbles));
		}

		#endregion

		//TODO: which game version is this for?
		public override void LoadTileEntities(ChunkData c, NBTCompound chunkNBT, Version? version)
		{
			c.tileEntities = new Dictionary<Coordinates.BlockCoord, TileEntity>();
			if (chunkNBT.Contains("TileEntities"))
			{
				var tileEntList = chunkNBT.GetAsList("TileEntities");
				if (tileEntList != null && tileEntList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < tileEntList.Length; i++)
					{
						var te = TileEntity.CreateFromNBT(tileEntList.Get<NBTCompound>(i), version, out var blockPos);
						c.tileEntities.Add(blockPos, te);
					}
				}
			}
		}

		//TODO: which game version is this for?
		public override void WriteTileEntities(ChunkData c, NBTCompound chunkNBT)
		{
			var comp = chunkNBT.AddList("TileEntities", NBTTag.TAG_Compound);
			foreach(var kv in c.tileEntities)
			{
				comp.Add(kv.Value.ToNBT(TargetVersion, kv.Key));
			}
		}

		public override void LoadEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion, Version? version)
		{
			c.entities = new List<Entity>();
			if (chunkNBT.TryGet<NBTList>("Entities", out var entList))
			{
				if (entList.contentsType == NBTTag.TAG_Compound)
				{
					for (int i = 0; i < entList.Length; i++)
					{
						c.entities.Add(new Entity(entList.Get<NBTCompound>(i)));
					}
				}
			}
		}

		protected override void PostWrite(ChunkData c, NBTCompound chunkNBT)
		{
			var hm = new byte[256];
			for (int z = 0; z < 16; z++)
			{
				for (int x = 0; x < 16; x++)
				{
					hm[z * 16 + x] = (byte)c.GetHighestBlock(x, z);
				}
			}
			chunkNBT.Add("HeightMap", hm);
		}

		public override void WriteEntities(ChunkData c, NBTCompound chunkNBT, Region parentRegion)
		{
			chunkNBT.Add("Entities", new NBTList(NBTTag.TAG_Compound));
			//TODO
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT, Version? version)
		{
			//Do nothing, saved biomes were not implemented back then, they were generated on the fly by the game (based on the world seed)
		}

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			//Do nothing, saved biomes were not implemented back then, they were generated on the fly by the game (based on the world seed)
		}

		public override void LoadTileTicks(ChunkData c, NBTCompound chunkNBT, Version? version)
		{
			//Do nothing, tile ticks were not implementen in old regions.
		}

		public override void WriteTileTicks(ChunkData c, NBTCompound chunkNBT)
		{
			//Do nothing, tile ticks were not implementen in old regions.
		}
	}
}

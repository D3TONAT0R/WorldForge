using System;
using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Builders
{
	public class Schematic
	{
		public class BlockDefinition
		{
			public BlockState block;
			public float probability = 1f;

			public BlockDefinition(BlockState block, float probability = 1f)
			{
				this.block = block;
				this.probability = probability;
			}

			public BlockDefinition(BlockID block, float probability = 1f)
			{
				this.block = new BlockState(block);
				this.probability = probability;
			}
		}

		public byte[,,] schematic;

		public int StructureSizeX => schematic.GetLength(0);
		public int StructureSizeY => schematic.GetLength(1);
		public int StructureSizeZ => schematic.GetLength(2);

		public List<BlockDefinition> palette = new List<BlockDefinition>();

		public BlockState trunkBlock = null;
		public byte trunkHeightMin = 1;
		public byte trunkHeightMax = 1;

		public Schematic(NBTCompound data)
		{
			var size = data.Get<NBTList>("Size");
			int sx = size.Get<int>(0);
			int sy = size.Get<int>(1);
			int sz = size.Get<int>(2);
			schematic = new byte[sx, sy, sz];
			var blocks = data.Get<NBTList>("Blocks");

		}

		public bool Build(Dimension dim, int x, int y, int z, Random r)
		{
			byte h = (byte)r.Next(trunkHeightMin, trunkHeightMax + 1);
			if(IsObstructed(dim, x, y + h, z))
			{
				return false;
			}
			if(trunkBlock != null && trunkHeightMax > 0)
			{
				for(int i = 0; i < h; i++)
				{
					dim.SetBlock((x, y + i, z), trunkBlock);
				}
			}
			int xm = x - (int)Math.Floor((float)StructureSizeX / 2);
			int zm = z - (int)Math.Floor((float)StructureSizeZ / 2);
			for(int x1 = 0; x1 < StructureSizeX; x1++)
			{
				for(int y1 = 0; y1 < StructureSizeY; y1++)
				{
					for(int z1 = 0; z1 < StructureSizeZ; z1++)
					{
						var d = schematic[x1, y1, z1];
						if(d == 0) continue;
						var def = palette[d];
						if(r.NextDouble() < def.probability)
						{
							dim.SetBlock((xm + x1, y + h + y1, zm + z1), def.block);
						}
					}
				}
			}
			return true;
		}

		private bool IsObstructed(Dimension dim, int lx, int ly, int lz)
		{
			int x1 = lx - (int)Math.Floor(StructureSizeX / 2f);
			int x2 = lx + (int)Math.Ceiling(StructureSizeX / 2f);
			int y1 = ly;
			int y2 = ly + StructureSizeY;
			int z1 = lz - (int)Math.Floor(StructureSizeZ / 2f);
			int z2 = lz + (int)Math.Ceiling(StructureSizeZ / 2f);
			int sy = 0;
			for(int y = y1; y < y2; y++)
			{
				int sz = 0;
				for(int z = z1; z < z2; z++)
				{
					int sx = 0;
					for(int x = x1; x < x2; x++)
					{
						if(schematic[sx, sy, sz] == 0) continue; //Do not check this block if the result is nothing anyway
						if(!dim.IsAirOrNull((x, y, z)) || dim.GetRegionAtBlock(x, z) == null) return true;
						sx++;
					}
					sz++;
				}
				sy++;
			}
			return false;
		}
	}
}

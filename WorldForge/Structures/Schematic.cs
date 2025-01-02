using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.TileEntities;

namespace WorldForge.Structures
{
	public class Schematic
	{
		public struct Block : INBTConverter
		{
			[NBT]
			public BlockState state;
			[NBT]
			public NBTCompound nbt;
			[NBT]
			public float probability;

			public Block(BlockState state, NBTCompound nbt = null, float probability = 1.0f)
			{
				this.state = state;
				this.nbt = nbt;
				this.probability = probability;
			}

			public Block(BlockState state, float probability) : this(state, null, probability)
			{

			}

			public Block(NBTCompound nbt) : this()
			{
				FromNBT(nbt);
			}

			public object ToNBT(GameVersion version) => NBTConverter.WriteToNBT(this, new NBTCompound(), version);

			public void FromNBT(object nbtData) => NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
		}

		public IStructurePalette<Block> paletteData = new StructurePalette<Block>();

		public Dictionary<BlockCoord, int> blocks = new Dictionary<BlockCoord, int>();

		public Dictionary<BlockCoord, int> treeTrunk = new Dictionary<BlockCoord, int>();
		public int trunkMinHeight = 0;
		public int trunkMaxHeight = 0;

		public static Schematic From3DArray(IStructurePalette<Block> palette, int[,,] indices, bool yzxOrder, BlockCoord originOffset)
		{
			var structure = new Schematic();
			structure.paletteData = palette;
			structure.blocks = Parse3DArray(indices, originOffset, yzxOrder);
			return structure;
		}

		public static Schematic From3DArray(IStructurePalette<Block> palette, int[,,] indices, bool yzxOrder, BlockCoord originOffset, int[,,] trunkIndices, int trunkMinHeight, int trunkMaxHeight)
		{
			var structure = new Schematic();
			structure.paletteData = palette;
			structure.blocks = Parse3DArray(indices, originOffset, yzxOrder);
			structure.treeTrunk = Parse3DArray(trunkIndices, originOffset, yzxOrder);
			structure.trunkMinHeight = trunkMinHeight;
			structure.trunkMaxHeight = trunkMaxHeight;
			return structure;
		}

		public static Schematic From3DArray(IStructurePalette<Block> palette, int[,,] indices, bool yzxOrder, BlockCoord originOffset, int trunkIndex, int trunkMinHeight, int trunkMaxHeight)
		{
			var structure = new Schematic();
			structure.paletteData = palette;
			structure.blocks = Parse3DArray(indices, originOffset, yzxOrder);
			structure.treeTrunk = new Dictionary<BlockCoord, int> { { new BlockCoord(0, 0, 0), trunkIndex } };
			structure.trunkMinHeight = trunkMinHeight;
			structure.trunkMaxHeight = trunkMaxHeight;
			return structure;
		}

		private static Dictionary<BlockCoord, int> Parse3DArray(int[,,] indices, BlockCoord originOffset, bool yzxOrder)
		{
			var data = new Dictionary<BlockCoord, int>();
			for(int z = 0; z < indices.GetLength(2); z++)
			{
				for(int y = 0; y < indices.GetLength(1); y++)
				{
					for(int x = 0; x < indices.GetLength(0); x++)
					{
						int i = indices[x, y, z];
						if(i >= 0)
						{
							var pos = (yzxOrder ? new BlockCoord(z, x, y) : new BlockCoord(x, y, z)) - originOffset;
							data[pos] = i;
						}
					}
				}
			}
			return data;
		}

		public void Build(Dimension dimension, BlockCoord origin, Random random, bool allowNewChunks = false)
		{
			int paletteIndex = paletteData.PaletteCount > 1 ? random.Next(paletteData.PaletteCount) : 0;
			var palette = paletteData.GetPalette(paletteIndex);

			//Build tree trunk (if applicable) and adjust origin for main structure (the top part)
			if(treeTrunk != null && treeTrunk.Count > 0 && trunkMaxHeight > 0)
			{
				int height = random.Next(trunkMinHeight, trunkMaxHeight + 1);
				for(int i = 0; i < height; i++)
				{
					Build(dimension, origin + new BlockCoord(0, i, 0), random, allowNewChunks, treeTrunk, palette);
				}
				origin.y += height;
			}

			//Build main structure
			Build(dimension, origin, random, allowNewChunks, blocks, palette);
		}

		private void Build(Dimension dimension, BlockCoord origin, Random random, bool allowNewChunks, Dictionary<BlockCoord, int> data, List<Block> palette)
		{
			foreach(var kv in data)
			{
				var pos = kv.Key;
				var index = kv.Value;
				var block = palette[index];

				if(random.NextDouble() <= block.probability)
				{
					dimension.SetBlock(origin + pos, block.state, allowNewChunks);
					if(block.nbt != null)
					{
						var tileEntity = TileEntity.CreateFromNBT(block.nbt, dimension.ParentWorld?.GameVersion, out _);
						dimension.SetTileEntity(origin + pos, tileEntity);
					}
				}
			}
		}
	}
}
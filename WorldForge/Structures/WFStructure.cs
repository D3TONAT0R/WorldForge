using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.TileEntities;

namespace WorldForge.Structures
{
	public class WFStructure
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

			public Block(BlockState index, float probability = 1.0f) : this(index, null, probability)
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
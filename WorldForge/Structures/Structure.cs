using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;
using WorldForge.TileEntities;

namespace WorldForge.Structures
{
	public class Structure : INBTConverter
	{
		public struct Block : INBTConverter
		{
			[NBT]
			public int state;
			[NBT]
			public BlockCoord pos;
			[NBT]
			public NBTCompound nbt;

			public Block(int state, BlockCoord pos, NBTCompound nbt = null)
			{
				this.state = state;
				this.pos = pos;
				this.nbt = nbt;
			}

			public Block(NBTCompound nbt) : this()
			{
				FromNBT(nbt);
			}

			public object ToNBT(GameVersion version) => NBTConverter.WriteToNBT(this, new NBTCompound(), version);

			public void FromNBT(object nbtData) => NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
		}

		public struct Entity : INBTConverter
		{
			[NBT]
			public Vector3 pos;
			[NBT]
			public BlockCoord blockPos;
			[NBT]
			public NBTCompound nbt;

			public Entity(Vector3 pos, BlockCoord blockPos, NBTCompound nbt)
			{
				this.pos = pos;
				this.blockPos = blockPos;
				this.nbt = nbt;
			}

			public Entity(NBTCompound nbt) : this()
			{
				FromNBT(nbt);
			}

			public object ToNBT(GameVersion version) => NBTConverter.WriteToNBT(this, new NBTCompound(), version);

			public void FromNBT(object nbtData) => NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
		}

		public int? dataVersion;
		[NBT("author", "a1.0.0", "1.13")]
		public string author;
		public IStructurePalette<BlockState> paletteData;
		[NBT]
		public List<Block> blocks = new List<Block>();
		[NBT]
		public List<Entity> entities = new List<Entity>();

		public Structure(NBTCompound nbt)
		{
			FromNBT(nbt);
		}

		public void Build(Dimension dimension, BlockCoord origin, Random random)
		{
			int paletteIndex = paletteData.PaletteCount > 1 ? random.Next(paletteData.PaletteCount) : 0;
			var palette = paletteData.GetPalette(paletteIndex);
			foreach(var block in blocks)
			{
				var state = palette[block.state];
				var pos = origin + block.pos;
				dimension.SetBlock(pos, state);
				if(block.nbt != null)
				{
					var tileEntity = TileEntity.CreateFromNBT(block.nbt, dimension.ParentWorld?.GameVersion, out _);
					dimension.SetTileEntity(pos, tileEntity);
				}
			}
			foreach(var entity in entities)
			{
				//TODO: Add support for entities
				//var pos = origin + entity.blockPos;
				//dimension.AddEntity(entity.nbt, pos, entity.pos);
			}
		}

		public object ToNBT(GameVersion version)
		{
			if(paletteData == null)
			{
				throw new InvalidOperationException("Structure palette is null.");
			}
			var compound = new NBTCompound();
			NBTConverter.WriteToNBT(this, compound, version);
			compound.Add("DataVersion", dataVersion ?? version.GetDataVersion() ?? GameVersion.FirstDataVersion);
			switch(paletteData)
			{
				case StructurePalette<BlockState> singlePalette:
					compound.Add("palette", singlePalette.ToNBT(version));
					break;
				case StructureMultiPalette<BlockState> multiPalette:
					compound.Add("palettes", multiPalette.ToNBT(version));
					break;
				default:
					throw new ArgumentException("Invalid palette type.");
			}
			return compound;
		}

		public void FromNBT(object nbtData)
		{
			var comp = (NBTCompound)nbtData;
			NBTConverter.LoadFromNBT(comp, this);
			if(comp.TryGet<NBTList>("palette", out var paletteList))
			{
				paletteData = new StructurePalette<BlockState>(paletteList);
			}
			else if(comp.TryGet<NBTList>("palettes", out var palettesList))
			{
				paletteData = new StructureMultiPalette<BlockState>(palettesList);
			}
			else
			{
				throw new ArgumentException("No palette found in structure NBT.");
			}
		}
	}
}
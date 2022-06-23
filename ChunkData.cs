using MCUtils.Coordinates;
using MCUtils.IO;
using System;
using System.Collections.Generic;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class ChunkData
	{
		public ChunkCoord coords;

		public string defaultBlock = "minecraft:stone";
		public bool unlimitedHeight = false; //Allow blocks below 0 and above 256 (Versions 1.17+)
		public Region containingRegion;
		public Dictionary<sbyte, ChunkSection> sections = new Dictionary<sbyte, ChunkSection>();
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }

		public List<Entity> entities = new List<Entity>();
		public Dictionary<BlockCoord, TileEntity> tileEntities = new Dictionary<BlockCoord, TileEntity>();

		public List<BlockCoord> postProcessTicks = new List<BlockCoord>();

		public long inhabitedTime = 0;

		public NBTContent sourceNBT;

		private readonly object lockObj = new object();

		public ChunkData(Region region, ChunkCoord pos, string defaultBlock)
		{
			containingRegion = region;
			coords = pos;
			this.defaultBlock = defaultBlock;
		}

		public ChunkData(Region region, NBTContent chunk, ChunkCoord chunkCoord)
		{
			containingRegion = region;
			sourceNBT = chunk;
			coords = chunkCoord;
		}

		///<summary>Sets the block at the given chunk coordinate</summary>
		public void SetBlockAt(int x, int y, int z, BlockState block)
		{
			if (!unlimitedHeight && (y < 0 || y > 255)) return;
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, block);
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)Math.Floor(y / 16f);
			if (!sections.ContainsKey(sectionY))
			{
				if (allowNew)
				{
					sections.Add(sectionY, new ChunkSection(defaultBlock));
					RecalculateSectionRange();
				}
				else
				{
					return null;
				}
			}
			return sections[sectionY];
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given chunk coordinate. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlockAt(int x, int y, int z)
		{
			GetChunkSectionForYCoord(y, true).SetBlockAt(x, y % 16, z, 1); //1 is always the default block in a region generated from scratch
		}

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlockAt(int x, int y, int z)
		{
			var sec = GetChunkSectionForYCoord(y, false);
			if (sec == null) return BlockState.Air;
			return sec.GetBlockAt(x, y % 16, z);
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(int x, int y, int z, TileEntity te)
		{
			if (tileEntities == null)
			{
				tileEntities = new Dictionary<BlockCoord, TileEntity>();
			}
			tileEntities.Add(new BlockCoord(x, y, z), te);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(int x, int y, int z)
		{
			if (tileEntities == null) return null;
			var c = new BlockCoord(x, y, z);
			if (tileEntities.ContainsKey(c))
			{
				return tileEntities[c];
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int y, int z, BiomeID biome)
		{
			var section = GetChunkSectionForYCoord(y, false);
			if(section != null)
			{
				section.SetBiomeAt(x, y % 16, z, biome);
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(int x, int z, BiomeID biome)
		{
			foreach(var sec in sections.Values)
			{
				sec.SetBiomeColumnAt(x, z, biome);
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(int x, int y, int z)
		{
			var section = GetChunkSectionForYCoord(y, false);
			if(section != null)
			{
				return section.GetBiomeAt(x, y % 16, z);
			}
			else
			{
				return BiomeID.plains;
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID GetBiomeAt(int x, int z)
		{
			return GetBiomeAt(x, HighestSection * 16 + 15, z);
		}

		/// <summary>
		/// Marks the given chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(int x, int y, int z)
		{
			var coord = new BlockCoord(x, y, z);
			if (!postProcessTicks.Contains(coord))
			{
				postProcessTicks.Add(coord);
			}
		}

		/// <summary>
		/// Unmarks a previously marked chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(int x, int y, int z)
		{
			var coord = new BlockCoord(x, y, z);
			if (postProcessTicks.Contains(coord))
			{
				postProcessTicks.Remove(coord);
			}
		}

		public short GetHighestBlock(int x, int z, HeightmapType type = HeightmapType.AllBlocks)
		{
			short y = (short)(HighestSection * 16 + 15);
			while (y >= LowestSection * 16)
			{
				if (Blocks.IsBlockForMap(GetBlockAt(x, y, z).block, type)) return y;
				y--;
			}
			return short.MinValue;
		}

		///<summary>Writes the chunk's height data to a large heightmap at the given chunk coords</summary>
		public void WriteToHeightmap(short[,] hm, int x, int z, HeightmapType type)
		{
			if (!WriteHeightmapFromNBT(hm, x, z, type))
			{
				WriteHeightmapFromBlocks(hm, x, z, type);
			}
		}

		public void RecalculateSectionRange()
		{
			sbyte? lowest = null;
			sbyte? highest = null;
			for (sbyte s = -100; s < 127; s++)
			{
				if (lowest == null && sections.ContainsKey(s))
				{
					lowest = s;
				}
				if (sections.ContainsKey(s))
				{
					highest = s;
				}
			}
			LowestSection = lowest ?? 0;
			HighestSection = highest ?? 0;
		}

		private bool WriteHeightmapFromNBT(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			if (sourceNBT == null) return false;
			var chunkHM = sourceNBT.GetHeightmapFromChunkNBT(type);
			if (chunkHM == null) return false;
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = chunkHM[x, z];
				}
			}
			return true;
		}

		private void WriteHeightmapFromBlocks(short[,] hm, int localChunkX, int localChunkZ, HeightmapType type)
		{
			/*sbyte highestSection = 127;
			while (highestSection > -127 && !sections.ContainsKey(highestSection))
			{
				highestSection--;
			}
			if (highestSection == -127) return;
			var sec = sections[highestSection];
			*/
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					hm[localChunkX * 16 + x, localChunkZ * 16 + z] = GetHighestBlock(x, z, type);
					/*
					short yTop;
					if (hm[x, z] != 0)
					{
						//The heightmap is already present, proceed from provided y value
						yTop = hm[x, z];
					}
					else
					{
						yTop = (short)(highestSection * 16 + 15);
					}
					for (short y = yTop; y > 0; y--)
					{
						var block = sec.GetBlockAt(x, y % 16, z);
						if (Blocks.IsBlockForMap(block.block, type))
						{
							if (block.block.Compare("minecraft:snow"))
							{
								//Go down to grass level in case of a snow layer
								y--;
							}
							hm[localChunkX * 16 + x, 511 - (localChunkZ * 16 + z)] = y;
							break;
						}
					}
					*/
				}
			}
		}

		private long BitsToLong(string bits)
		{
			bits = bits.PadLeft(64, '0');
			return Convert.ToInt64(bits, 2);
		}

		//TODO: How to deal with negative section Y values? (Minecraft 1.17+)
		private CompoundContainer GetSectionCompound(ListContainer sectionsList, sbyte y)
		{
			foreach (var o in sectionsList.cont)
			{
				var compound = (CompoundContainer)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				if ((byte)compound.Get("Y") == y) return compound;
			}
			return null;
		}
	}
}
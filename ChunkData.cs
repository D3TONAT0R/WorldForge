using MCUtils.Coordinates;
using MCUtils.NBT;
using MCUtils.TileEntities;
using System;
using System.Collections.Generic;

namespace MCUtils
{
	public class ChunkData
	{
		public ChunkCoord worldSpaceCoord;

		public ChunkCoord RegionSpaceCoord => new ChunkCoord(worldSpaceCoord.x.Mod(32), worldSpaceCoord.z.Mod(32));

		public ChunkStatus status = ChunkStatus.light;

		public string defaultBlock = "minecraft:stone";
		public Region containingRegion;
		public Dictionary<sbyte, ChunkSection> sections = new Dictionary<sbyte, ChunkSection>();
		public sbyte HighestSection { get; private set; }
		public sbyte LowestSection { get; private set; }

		public List<Entity> entities = new List<Entity>();
		public Dictionary<BlockCoord, TileEntity> tileEntities = new Dictionary<BlockCoord, TileEntity>();

		public List<BlockCoord> postProcessTicks = new List<BlockCoord>();

		public long inhabitedTime = 0;

		public NBTFile sourceNBT;

		public bool HasTerrain => status >= ChunkStatus.surface;
		public bool HasFullyGeneratedTerrain => status >= ChunkStatus.light;

		private readonly object lockObj = new object();

		public ChunkData(Region region, ChunkCoord pos, string defaultBlock)
		{
			containingRegion = region;
			worldSpaceCoord = pos;
			this.defaultBlock = defaultBlock;
		}

		public ChunkData(Region region, NBTFile chunk, ChunkCoord chunkCoord)
		{
			containingRegion = region;
			sourceNBT = chunk;
			worldSpaceCoord = chunkCoord;
		}

		///<summary>Sets the block at the given chunk coordinate</summary>
		public void SetBlockAt(BlockCoord pos, BlockState block)
		{
			GetChunkSectionForYCoord(pos.y, true).SetBlockAt(pos.x, pos.y.Mod(16), pos.z, block);
		}

		public ChunkSection GetChunkSectionForYCoord(int y, bool allowNew)
		{
			sbyte sectionY = (sbyte)Math.Floor(y / 16f);
			if (!sections.ContainsKey(sectionY))
			{
				if (allowNew)
				{
					sections.Add(sectionY, new ChunkSection(this, defaultBlock));
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
		public void SetDefaultBlockAt(BlockCoord pos)
		{
			GetChunkSectionForYCoord(pos.y, true).SetBlockAt(pos.x, pos.y.Mod(16), pos.z, 1); //1 is always the default block in a region generated from scratch
		}

		///<summary>Gets the block at the given chunk coordinate</summary>
		public BlockState GetBlockAt(BlockCoord pos)
		{
			if(!HasTerrain) return null;
			var sec = GetChunkSectionForYCoord(pos.y, false);
			if (sec == null) return null;
			return sec.GetBlockAt(pos.x, pos.y.Mod(16), pos.z);
		}

		public int ForEachBlock(short yMin, short yMax, Action<BlockCoord, BlockState> action)
		{
			if(!HasTerrain) return 0;
			int countedBlocks = 0;
			foreach(var kv in sections)
			{
				var section = kv.Value;
				short baseY = (short)(kv.Key * 16);
				if(baseY > yMax || baseY + 15 < yMin) continue;

				for(byte y = 0; y < 16; y++)
				{
					short worldY = (short)(baseY + y);
					if(worldY < yMin || worldY >= yMax) continue;
					for(byte z = 0; z < 16; z++)
					{
						for(byte x = 0; x < 16; x++)
						{
							var b = section.GetBlockAt(x, y, z);
							action.Invoke(new BlockCoord(x, baseY + y, z), b);
							countedBlocks++;
						}
					}
				}
			}
			return countedBlocks;
		}

		public int ForEachBlock(Action<BlockCoord, BlockState> action)
		{
			return ForEachBlock((short)(LowestSection * 16), (short)(HighestSection * 16 + 15), action);
		}

		///<summary>Sets the tile entity for the block at the given chunk coordinate.</summary>
		public void SetTileEntity(BlockCoord pos, TileEntity te)
		{
			if (tileEntities == null)
			{
				tileEntities = new Dictionary<BlockCoord, TileEntity>();
			}
			tileEntities.Add(pos, te);
		}

		///<summary>Gets the tile entity for the block at the given chunk coordinate (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			if (tileEntities == null) return null;
			if (tileEntities.ContainsKey(pos))
			{
				return tileEntities[pos];
			}
			else
			{
				return null;
			}
		}

		///<summary>Sets the biome at the given chunk coordinate</summary>
		public void SetBiomeAt(BlockCoord pos, BiomeID biome)
		{
			var section = GetChunkSectionForYCoord(pos.y, false);
			if(section != null)
			{
				section.SetBiomeAt(pos.x, pos.y.Mod(16), pos.z, biome);
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
		public BiomeID? GetBiomeAt(BlockCoord pos)
		{
			var section = GetChunkSectionForYCoord(pos.y, false);
			if(section != null)
			{
				return section.GetBiomeAt(pos.x, pos.y.Mod(16), pos.z);
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the biome at the given chunk coordinate</summary>
		public BiomeID? GetBiomeAt(int x, int z)
		{
			sbyte highestSectionWithBiomeData = HighestSection;
			while(highestSectionWithBiomeData > 0 && (!sections.TryGetValue(highestSectionWithBiomeData, out var s) || !s.HasBiomesDefined))
			{
				highestSectionWithBiomeData--;
			}
			return GetBiomeAt((x, highestSectionWithBiomeData * 16 + 15, z));
		}

		/// <summary>
		/// Marks the given chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			if (!postProcessTicks.Contains(pos))
			{
				postProcessTicks.Add(pos);
			}
		}

		/// <summary>
		/// Unmarks a previously marked chunk coordinate to be ticked when this chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			if (postProcessTicks.Contains(pos))
			{
				postProcessTicks.Remove(pos);
			}
		}

		public short GetHighestBlock(int x, int z, HeightmapType type = HeightmapType.AllBlocks)
		{
			short y = (short)(HighestSection * 16 + 15);
			while (y >= LowestSection * 16)
			{
				var blockState = GetBlockAt((x.Mod(16), y, z.Mod(16)));
				if(blockState != null && Blocks.IsBlockForMap(blockState.block, type)) return y;
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
						var block = sec.GetBlockAt(x, y.Mod(16), z);
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

		//TODO: How to deal with negative section Y values? (Minecraft 1.17+)
		private NBTCompound GetSectionCompound(NBTList sectionsList, sbyte y)
		{
			foreach (var o in sectionsList.listContent)
			{
				var compound = (NBTCompound)o;
				if (!compound.Contains("Y") || !compound.Contains("Palette")) continue;
				if ((byte)compound.Get("Y") == y) return compound;
			}
			return null;
		}
	}
}
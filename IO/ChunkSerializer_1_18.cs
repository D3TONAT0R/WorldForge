﻿using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public class ChunkSerializer_1_18 : ChunkSerializer_1_16
	{
		public override bool AddRootLevelCompound => false;

		public ChunkSerializer_1_18(Version version) : base(version) { }

		protected override bool HasBlocks(NBTContent.CompoundContainer sectionNBT)
		{
			return sectionNBT.Contains("block_states");
		}

		protected override ListContainer GetSectionsList(CompoundContainer chunkNBT)
		{
			return chunkNBT.GetAsList("sections");
		}

		protected override ListContainer GetBlockPalette(NBTContent.CompoundContainer sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").GetAsList("palette");
		}

		protected override long[] GetBlockDataArray(CompoundContainer sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").Get<long[]>("data");
		}

		public override void LoadBiomes(ChunkData c, CompoundContainer chunkNBT)
		{
			var sectionsList = chunkNBT.GetAsList("sections");
			foreach(var s in sectionsList.cont)
			{
				var sectionNBT = (CompoundContainer)s;
				if(sectionNBT.TryGet<CompoundContainer>("biomes", out var biomesComp))
				{
					var paletteNBT = biomesComp.GetAsList("palette");
					BiomeID[] palette = new BiomeID[paletteNBT.Length];
					for (int i = 0; i < paletteNBT.Length; i++)
					{
						var id = paletteNBT.Get<string>(i).Replace("minecraft:", "");
						if(Enum.TryParse<BiomeID>(id, out var b))
						{
							palette[i] = b;
						}
						else
						{
							Console.WriteLine("Unrecognized biome: " + id);
							palette[i] = BiomeID.plains;
						}
					}

					sbyte secY;
					unchecked {
						secY = (sbyte)sectionNBT.Get<byte>("Y");
					}
					var chunkSection = c.sections[secY];

					if(palette.Length > 1)
					{
						var biomeData = biomesComp.Get<long[]>("data");
						var indexBitLength = BitUtils.GetMaxBitCount((uint)palette.Length - 1);
						var dataBits = BitUtils.CreateBitArray(biomeData);
						for (int y = 0; y < 4; y++)
						{
							for (int z = 0; z < 4; z++)
							{
								for (int x = 0; x < 4; x++)
								{
									int i = y * 16 + z * 4 + x;
									i *= indexBitLength;
									var paletteIndex = BitUtils.CreateInt16FromBits(dataBits, i, (int)indexBitLength);
									//HACK: bypassed exception throw due to incorrect parsing
									try
									{
										chunkSection.SetBiome3D4x4At(x * 4, y * 4, z * 4, palette[paletteIndex]);
									}
									catch
									{

									}
								}
							}
						}
					}
					else
					{
						if(palette[0] != BiomeID.plains)
						{
							for (int y = 0; y < 16; y+=4)
							{
								for (int z = 0; z < 16; z += 4)
								{
									for (int x = 0; x < 16; x += 4)
									{
										chunkSection.SetBiome3D4x4At(x, y, z, palette[0]);
									}
								}
							}
						}
						else
						{
							//Do nothing, as the entire chunk is the default (plains) biome
						}
					}
				}
			}
		}

		public override void WriteBiomes(ChunkData c, CompoundContainer chunkNBT)
		{
			//TODO
		}
	}
}

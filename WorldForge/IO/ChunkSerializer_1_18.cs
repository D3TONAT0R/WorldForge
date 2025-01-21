using System;
using System.Collections.Generic;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_18 : ChunkSerializer_1_17
	{
		public override bool AddRootLevelCompound => false;
		public override string SectionsCompName => "sections";
		public override string BlocksCompName => "block_states";
		public override string BlockDataCompName => "data";
		public override string BiomesCompName => "biomes";
		public override string TileEntitiesCompName => "block_entities";

		public ChunkSerializer_1_18(GameVersion version) : base(version) { }

		public override NBTCompound GetRootCompound(NBTFile chunkNBTData) => chunkNBTData.contents;

		protected override bool SectionHasBlocks(NBTCompound sectionNBT)
		{
			return sectionNBT.Contains("block_states");
		}

		protected override NBTList GetBlockPalette(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").GetAsList("palette");
		}

		protected override long[] GetBlockDataArray(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").Get<long[]>("data");
		}

		public override void LoadBiomes(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{
			var sectionsList = chunkNBT.GetAsList(SectionsCompName);
			foreach(var s in sectionsList.listContent)
			{
				var sectionNBT = (NBTCompound)s;
				if(sectionNBT.TryGet<NBTCompound>(BiomesCompName, out var biomesComp))
				{
					var paletteNBT = biomesComp.GetAsList("palette");
					BiomeID[] palette = new BiomeID[paletteNBT.Length];
					for(int i = 0; i < paletteNBT.Length; i++)
					{
						var id = paletteNBT.Get<string>(i).Replace("minecraft:", "");
						if(BiomeIDs.TryGet(id, out var b))
						{
							palette[i] = b;
						}
						else
						{
							Logger.Warning($"Unrecognized biome '{id}', adding to list.");
							BiomeID newBiome = BiomeIDs.GetOrCreate(id);
							palette[i] = newBiome;
						}
					}

					sbyte secY;
					unchecked
					{
						secY = (sbyte)sectionNBT.Get<byte>("Y");
					}
					var chunkSection = c.Sections[secY];

					if(palette.Length > 1)
					{
						var biomeData = biomesComp.Get<long[]>(BlockDataCompName);
						var indexBitLength = BitUtils.GetMaxBitCount((uint)palette.Length - 1);
						var indices = BitUtils.UnpackBits(biomeData, indexBitLength, 64, false);

						for(int y = 0; y < 4; y++)
						{
							for(int z = 0; z < 4; z++)
							{
								for(int x = 0; x < 4; x++)
								{
									int i = y * 16 + z * 4 + x;
									var paletteIndex = indices[i];
									chunkSection.SetBiome3D4x4(x * 4, y * 4, z * 4, palette[paletteIndex]);
								}
							}
						}
					}
					else
					{
						if(palette[0] != BiomeID.Plains)
						{
							for(int y = 0; y < 16; y += 4)
							{
								for(int z = 0; z < 16; z += 4)
								{
									for(int x = 0; x < 16; x += 4)
									{
										chunkSection.SetBiome3D4x4(x, y, z, palette[0]);
									}
								}
							}
						}
						else
						{
							//Do nothing, as the entire chunk is the default biome
						}
					}
				}
			}
		}

		public override void WriteCommonData(Chunk c, NBTCompound chunkNBT)
		{
			base.WriteCommonData(c, chunkNBT);
			chunkNBT.Add("yPos", (sbyte)-4);
		}

		public override void WriteSection(ChunkSection section, NBTCompound comp, sbyte sectionY)
		{
			WriteSectionBlocks(section, comp);
			WriteSectionBiomes(section, comp);
			if(section.lighting != null)
			{
				//TODO: write light information and find a way to omit sky or block light
				//WriteSectionLightmaps(section, comp);
			}
		}

		private void WriteSectionBlocks(ChunkSection section, NBTCompound comp)
		{
			var blockStates = comp.AddCompound("block_states");
			var blockPalette = blockStates.AddList("palette", NBTTag.TAG_Compound);
			foreach(var b in section.palette)
			{
				var b1 = b;
				BlockState.ResolveBlockState(TargetVersion, ref b1);
				blockPalette.Add(b1.ToNBT(TargetVersion));
			}
			var blockData = GetBlockIndexArray(section);
			int bitsPerBlock = Math.Max(4, BitUtils.GetMaxBitCount((uint)blockPalette.Length - 1));
			blockStates.Add("data", BitUtils.PackBits(blockData, bitsPerBlock, false));
		}

		private void WriteSectionBiomes(ChunkSection section, NBTCompound comp)
		{
			var biomes = comp.AddCompound("biomes");
			var biomePalette = biomes.AddList("palette", NBTTag.TAG_String);
			if(section.HasBiomesDefined)
			{
				List<BiomeID> biomePaletteList = new List<BiomeID>();
				ushort[] biomeIndexData = new ushort[4 * 4 * 4];
				for(int y = 0; y < 4; y++)
				{
					for(int z = 0; z < 4; z++)
					{
						for(int x = 0; x < 4; x++)
						{
							int i = y * 16 + z * 4 + x;
							var biome = section.GetPredominantBiomeAt4x4(x, y, z);
							if(!biomePaletteList.Contains(biome))
							{
								biomePaletteList.Add(biome);
							}
							biomeIndexData[i] = (byte)biomePaletteList.IndexOf(biome);
						}
					}
				}
				if(biomePaletteList.Count == 1)
				{
					//No need to save the data if there's only one biome in the palette
					biomePalette.Add(biomePaletteList[0].ResolveIDForVersion(TargetVersion));
				}
				else
				{
					foreach(var b in biomePaletteList)
					{
						biomePalette.Add(b.ResolveIDForVersion(TargetVersion));
					}
					biomes.Add("data", BitUtils.PackBits(biomeIndexData, BitUtils.GetMaxBitCount((uint)biomePaletteList.Count - 1), false));
				}
			}
			else
			{
				//Write default biome
				var defaultBiome = section.chunk?.ParentDimension?.DefaultBiome ?? BiomeID.Plains;
				biomePalette.Add(defaultBiome.ResolveIDForVersion(TargetVersion));
			}
		}
	}
}

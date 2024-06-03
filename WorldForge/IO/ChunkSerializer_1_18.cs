using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_18 : ChunkSerializer_1_16
	{
		public override bool AddRootLevelCompound => false;

		public ChunkSerializer_1_18(GameVersion version) : base(version) { }

		public override NBTCompound GetRootCompound(NBTFile chunkNBTData) => chunkNBTData.contents;

		protected override bool HasBlocks(NBTCompound sectionNBT)
		{
			return sectionNBT.Contains("block_states");
		}

		protected override NBTList GetSectionsList(NBTCompound chunkNBT)
		{
			return chunkNBT.GetAsList("sections");
		}

		protected override NBTList GetBlockPalette(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").GetAsList("palette");
		}

		protected override long[] GetBlockDataArray(NBTCompound sectionNBT)
		{
			return sectionNBT.GetAsCompound("block_states").Get<long[]>("data");
		}

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			var sectionsList = chunkNBT.GetAsList("sections");
			foreach(var s in sectionsList.listContent)
			{
				var sectionNBT = (NBTCompound)s;
				if(sectionNBT.TryGet<NBTCompound>("biomes", out var biomesComp))
				{
					var paletteNBT = biomesComp.GetAsList("palette");
					BiomeID[] palette = new BiomeID[paletteNBT.Length];
					for(int i = 0; i < paletteNBT.Length; i++)
					{
						var id = paletteNBT.Get<string>(i).Replace("minecraft:", "");
						if(BiomeIDResolver.TryParseBiome(id, out var b))
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
					unchecked
					{
						secY = (sbyte)sectionNBT.Get<byte>("Y");
					}
					var chunkSection = c.sections[secY];

					if(palette.Length > 1)
					{
						var biomeData = biomesComp.Get<long[]>("data");
						var indexBitLength = BitUtils.GetMaxBitCount((uint)palette.Length - 1);
						var indices = BitUtils.ExtractCompressedInts(biomeData, indexBitLength, 64, false);

						for(int y = 0; y < 4; y++)
						{
							for(int z = 0; z < 4; z++)
							{
								for(int x = 0; x < 4; x++)
								{
									int i = y * 16 + z * 4 + x;
									var paletteIndex = indices[i];
									chunkSection.SetBiome3D4x4At(x * 4, y * 4, z * 4, palette[paletteIndex]);
								}
							}
						}
					}
					else
					{
						if(palette[0] != BiomeID.plains)
						{
							for(int y = 0; y < 16; y += 4)
							{
								for(int z = 0; z < 16; z += 4)
								{
									for(int x = 0; x < 16; x += 4)
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
	}
}

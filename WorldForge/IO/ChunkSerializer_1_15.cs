using System;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_15 : ChunkSerializer_1_13
	{
		public ChunkSerializer_1_15(GameVersion version) : base(version) { }

		public override void LoadBiomes(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			if(chunkNBT.TryGet<int[]>("Biomes", out var biomeData))
			{
				for(int y = 0; y < 64; y++)
				{
					for(int x = 0; x < 4; x++)
					{
						for(int z = 0; z < 4; z++)
						{
							int i = y * 16 + z * 4 + x;
							var biome = BiomeIDs.GetFromNumeric((byte)biomeData[i]);
							var section = c.GetChunkSectionForYCoord(y * 4, false);
							if(section != null)
							{
								section.SetBiome3D4x4At(x * 4, y * 4, z * 4, biome);
							}
						}
					}
				}
			}
		}

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
			//TODO: check if the biomes exists in the target version & replace them if necessary
			int[] biomeData = new int[1024];
			for(int y = 0; y < 64; y++)
			{
				for(int x = 0; x < 4; x++)
				{
					for(int z = 0; z < 4; z++)
					{
						int i = y * 16 + z * 4 + x;
						var section = c.GetChunkSectionForYCoord(y * 4, false);
						if(section != null)
						{
							var biome = section.GetPredominantBiomeAt4x4(x, y.Mod(4), z);
							BiomeID.Resolve(TargetVersion, ref biome);
							if(biome == null) biome = BiomeID.Plains;
							biomeData[i] = (int)biome.numericId;
						}
						else
						{
							//Repeat bottom- / topmost biome for the entire chunk
							if(c.Sections.Count > 0)
							{
								int sy = (sbyte)Math.Floor(y * 4 / 16f);
								bool above = sy > c.HighestSection;
								var otherSection =  c.Sections[above ? c.HighestSection : c.LowestSection];
								biomeData[i] = (int)otherSection.GetPredominantBiomeAt4x4(x, above ? 3 : 0, z).numericId;
							}
							else
							{
								//Write the default biome
								biomeData[i] = (int)(section.containingChunk.ParentDimension?.DefaultBiome.numericId ?? BiomeID.TheVoid.numericId);
							}
						}
					}
				}
			}
			chunkNBT.Add("Biomes", biomeData);
		}
	}
}

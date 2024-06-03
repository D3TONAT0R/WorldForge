using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_15 : ChunkSerializer_1_13
	{
		public ChunkSerializer_1_15(GameVersion version) : base(version) { }

		public override void WriteBiomes(ChunkData c, NBTCompound chunkNBT)
		{
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
							biomeData[i] = (int)section.GetPredominantBiomeAt4x4(x, y.Mod(4), z);
						}
						else
						{
							biomeData[i] = (int)BiomeID.plains;
						}
					}
				}
			}
			chunkNBT.Add("Biomes", biomeData);
		}
	}
}

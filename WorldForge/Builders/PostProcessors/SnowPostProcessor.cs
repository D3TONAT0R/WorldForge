using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge.Biomes;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class SnowPostProcessor : PostProcessor
	{
		private bool topOnly = true;
		private bool biomeCheck = true;

		private readonly BlockState snowLayerBlock = new BlockState("snow");
		private readonly BlockState iceBlock = new BlockState("ice");

		private readonly BlockState snowyGrass = new BlockState("grass_block", ("snowy", "true"));
		private readonly BlockState snowyPodzol = new BlockState("podzol", ("snowy", "true"));
		private readonly BlockState snowyMycelium = new BlockState("mycelium", ("snowy", "true"));

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		private static readonly Dictionary<BiomeID, short> snowThresholds = new Dictionary<BiomeID, short>()
		{
			{BiomeIDs.Get("snowy_tundra"), -999},
			{BiomeIDs.Get("snowy_plains"), -999},
			{BiomeIDs.Get("ice_spikes"), -999 },
			{BiomeIDs.Get("snowy_taiga"), -999 },
			{BiomeIDs.Get("snowy_taiga_hills"), -999 },
			{BiomeIDs.Get("snowy_taiga_mountains"), -999 },
			{BiomeIDs.Get("snowy_mountains"), -999 },
			{BiomeIDs.Get("snowy_beach"), -999 },
			{BiomeIDs.Get("gravelly_mountains"), 128 },
			{BiomeIDs.Get("modified_gravelly_mountains"), 128 },
			{BiomeIDs.Get("mountains"), 128 },
			{BiomeIDs.Get("mountain_edge"), 128 },
			{BiomeIDs.Get("taiga_mountains"), 128 },
			{BiomeIDs.Get("wooded_mountains"), 128 },
			{BiomeIDs.Get("stone_shore"), 128 },
			{BiomeIDs.Get("taiga"), 168 },
			{BiomeIDs.Get("taiga_hills"), 168 },
			{BiomeIDs.Get("giant_spruce_taiga"), 168 },
			{BiomeIDs.Get("giant_spruce_taiga_hills"), 168 },
			{BiomeIDs.Get("frozen_ocean"), 72 },
			{BiomeIDs.Get("deep_frozen_ocean"), 72 },
		};

		public SnowPostProcessor()
		{

		}

		public SnowPostProcessor(string rootPath, XElement xml) : base(rootPath, xml)
		{
			xml.TryParseBool("top-only", ref topOnly);
			xml.TryParseBool("check-biomes", ref biomeCheck);
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			var biome = dimension.GetBiome(pos);
			if(biome != null)
			{
				if(!topOnly)
				{
					FreezeBlock(dimension, pos, Seed + 1234, mask, biome);
				}
				int y2 = dimension.GetHighestBlock(pos.x, pos.z, HeightmapType.SolidBlocks);
				if(topOnly || y2 > pos.y)
				{
					FreezeBlock(dimension, new BlockCoord(pos.x, y2, pos.z), Seed + 1234, mask, biome);
				}
			}
		}

		private bool IsAboveBiomeThreshold(BiomeID biome, int y)
		{
			if(snowThresholds.TryGetValue(biome, out short threshold))
			{
				return y >= threshold;
			}
			else
			{
				//If the biome doesn't exist in the dictionary, it can't generate snow.
				return false;
			}
		}

		private void FreezeBlock(Dimension dim, BlockCoord pos, long seed, float mask, BiomeID biome, bool airCheck = true)
		{
			if(biome != null && !IsAboveBiomeThreshold(biome, pos.y)) return;
			bool canFreeze = !airCheck || dim.IsAirOrNull(pos.Above);
			if(!canFreeze) return;
			var block = dim.GetBlock(pos);
			if(block.IsWater)
			{
				//100% ice coverage above mask values of 0.25f
				if(mask >= 1 || SeededRandom.Probability(mask * 4f, seed, pos))
				{
					dim.SetBlock(pos, iceBlock);
				}
			}
			else
			{
				//if (mask >= 1 || random.NextDouble() <= mask)
				//{
				if(block.IsLiquid || block.CompareMultiple("minecraft:snow", "minecraft:ice")) return;
				dim.SetBlock(pos.Above, snowLayerBlock);
				//Add "snowy" tag on blocks that support it.
				if(block == snowyGrass.Block)
				{
					dim.SetBlock(pos, snowyGrass);
				}
				else if(block == snowyPodzol.Block)
				{
					dim.SetBlock(pos, snowyPodzol);
				}
				else if(block == snowyMycelium.Block)
				{
					dim.SetBlock(pos, snowyMycelium);
				}
				//}
			}
		}
	}
}
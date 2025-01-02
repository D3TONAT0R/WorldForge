using System;
using System.Xml.Linq;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using WorldForge.Structures;

namespace WorldForge.Builders.PostProcessors
{
	public class VegetationGenerator : PostProcessor
	{
		public enum GeneratorType : byte
		{
			None = 0,
			GrassOnly = 1,
			Plains = 2,
			Desert = 3,
			ColdPlains = 4,
			Forest = 5, 
			BirchForest = 6,
			SpruceForest = 7,
		}

		private const float CHUNK_MULTIPLIER = 1f / 128f;

		private const int _ = -1;

		private const int oakTrunkMinHeight = 1;
		private const int oakTrunkMaxHeight = 3;
		private const int birchTrunkMinHeight = 1;
		private const int birchTrunkMaxHeight = 4;
		private const int spruceTrunkMinHeight = 1;
		private const int spruceTrunkMaxHeight = 4;

		private static readonly int[,,] blueprintOakTreeTop = new int[,,] {
		//YZX
		{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,0,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_},
		},{
			{_,1,1,1,_},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{_,1,1,1,_}
		},{
			{_,1,1,1,_},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{_,1,1,1,_}
		},{
			{_,_,_,_,_},
			{_,1,1,1,_},
			{_,1,0,1,_},
			{_,1,1,1,_},
			{_,_,_,_,_}
		},{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,1,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_}
			}
		};

		private static readonly int[,,] blueprintSpruceTree = new int[,,] {
			//YZX
		{
			{_,1,1,1,_},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{_,1,1,1,_}
		},{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,0,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_}
		},{
			{_,1,1,1,_},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{_,1,1,1,_}
		},{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,0,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_}
		},{
			{_,1,1,1,_},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{_,1,1,1,_}
		},{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,1,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_}
		},{
			{_,_,_,_,_},
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,_,_,_,_},
			{_,_,_,_,_},
		},{
			{_,_,_,_,_},
			{_,_,1,_,_},
			{_,1,1,1,_},
			{_,_,1,_,_},
			{_,_,_,_,_}
		}
		};

		private static readonly StructurePalette<WFStructure.Block> oakTreePalette = new StructurePalette<WFStructure.Block>(
			new WFStructure.Block(new BlockState("oak_log")),
			new WFStructure.Block(new BlockState("oak_leaves"))
		);
		private readonly WFStructure oakTree = WFStructure.From3DArray(oakTreePalette, blueprintOakTreeTop, true, new BlockCoord(2, 0, 2), 0, oakTrunkMinHeight, oakTrunkMaxHeight);

		private static readonly StructurePalette<WFStructure.Block> birchTreePalette = new StructurePalette<WFStructure.Block>(
			new WFStructure.Block(new BlockState("birch_log")),
			new WFStructure.Block(new BlockState("birch_leaves"))
		);
		private readonly WFStructure birchTree = WFStructure.From3DArray(birchTreePalette, blueprintOakTreeTop, true, new BlockCoord(2, 0, 2), 0, birchTrunkMinHeight, birchTrunkMaxHeight);

		private static readonly StructurePalette<WFStructure.Block> spruceTreePalette = new StructurePalette<WFStructure.Block>(
			new WFStructure.Block(new BlockState("spruce_log")),
			new WFStructure.Block(new BlockState("spruce_leaves"))
		);
		private readonly WFStructure spruceTree = WFStructure.From3DArray(spruceTreePalette, blueprintSpruceTree, true, new BlockCoord(2, 0, 2), 0, spruceTrunkMinHeight, spruceTrunkMaxHeight);

		public GeneratorType FixedGeneratorType { get; set; } = GeneratorType.None;

		public float GrassDensity { get; set; } = 0.15f;
		public float GeneralTreesPerChunk { get; set; } = 0.03f;
		public float ForestTreesPerChunk { get; set; } = 2.5f;
		public float CactiPerChunk { get; set; } = 0.25f;
		public float DeadBushPerChunk { get; set; } = 0.3f;

		private readonly BlockID grassBlock = BlockList.Find("grass_block");
		private readonly BlockID dirtBlock = BlockList.Find("dirt");
		private readonly BlockID sandBlock = BlockList.Find("sand");
		private readonly BlockID grass = BlockList.Find("grass");
		private readonly BlockID deadBush = BlockList.Find("dead_bush");
		private readonly BlockID cactus = BlockList.Find("cactus");

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public VegetationGenerator()
		{

		}

		public VegetationGenerator(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			GrassDensity = float.Parse(xml.Element("grass")?.Value ?? "0.2");
			GeneralTreesPerChunk = float.Parse(xml.Element("trees")?.Value ?? "0.3") / 128f;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			var gen = FixedGeneratorType == GeneratorType.None ? GetBiomeType(dimension, pos) : FixedGeneratorType;
			switch(gen)
			{
				case GeneratorType.None:
					break;
				case GeneratorType.GrassOnly:
					TrySpawnGrass(dimension, pos);
					break;
				case GeneratorType.Plains:

					if(!TrySpawnTree(dimension, pos, oakTree, GeneralTreesPerChunk))
					{
						TrySpawnGrass(dimension, pos);
					}
					break;
				case GeneratorType.Desert:
					if(!TrySpawnCactus(dimension, pos, CactiPerChunk))
					{
						TrySpawnDeadBush(dimension, pos, DeadBushPerChunk * CHUNK_MULTIPLIER);
					}
					break;
				case GeneratorType.ColdPlains:
					if(!TrySpawnTree(dimension, pos, oakTree, GeneralTreesPerChunk))
					{
						TrySpawnGrass(dimension, pos);
					}
					break;
				case GeneratorType.Forest:
					if(!TrySpawnTree(dimension, pos, oakTree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, pos, GrassDensity * 0.5f);
					}
					break;
				case GeneratorType.BirchForest:
					if(!TrySpawnTree(dimension, pos, birchTree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, pos, GrassDensity * 0.5f);
					}
					break;
				case GeneratorType.SpruceForest:
					if(!TrySpawnTree(dimension, pos, spruceTree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, pos, GrassDensity * 0.5f);
					}
					break;
			}
		}

		private void TrySpawnGrass(Dimension dim, BlockCoord ground, float? probability = null)
		{
			if(Probability(probability ?? GrassDensity)) PlaceGrass(dim, ground.Above, grass);
		}

		private void TrySpawnDeadBush(Dimension dim, BlockCoord ground, float probability)
		{
			if(Probability(probability)) PlaceDeadBush(dim, ground.Above);
		}

		private bool TrySpawnTree(Dimension dim, BlockCoord ground, WFStructure tree, float amountPerChunk)
		{
			if(tree == null) return false;
			if(Probability(amountPerChunk * CHUNK_MULTIPLIER))
			{
				if(PlaceTree(dim, ground.Above, tree))
				{
					return true;
				}
			}
			return false;
		}

		private bool TrySpawnCactus(Dimension dim, BlockCoord ground, float amountPerChunk)
		{
			if(Probability(amountPerChunk * CHUNK_MULTIPLIER))
			{
				return PlaceCactus(dim, ground.Above);
			}
			return false;
		}

		private bool PlaceTree(Dimension dim, BlockCoord pos, WFStructure tree)
		{
			if(!Check(dim, pos.Below, grassBlock, dirtBlock) || !dim.IsAirOrNull(pos.Above)) return false;
			//if(IsObstructed(region, x, y+1, z, x, y+bareTrunkHeight, z) || IsObstructed(region, x-w, y+bareTrunkHeight, z-w, x+w, y+bareTrunkHeight+treeTopHeight, z+w)) return false;
			dim.SetBlock(pos.Below, "minecraft:dirt");
			tree.Build(dim, pos, random);
			return true;
		}

		private bool PlaceGrass(Dimension dim, BlockCoord pos, BlockID block)
		{
			if(Check(dim, pos.Below, grassBlock, dirtBlock)) return dim.SetBlock(pos, block);
			return false;
		}

		private bool PlaceDeadBush(Dimension dim, BlockCoord pos)
		{
			if(Check(dim, pos.Below, sandBlock)) return dim.SetBlock(pos, deadBush);
			return false;
		}

		private bool PlaceCactus(Dimension dim, BlockCoord pos)
		{
			if(Check(dim, pos.Below, sandBlock) && dim.IsAirOrNull(pos.Above))
			{
				int height = random.Next(1, 4);
				for(int y = 0; y < height; y++)
				{
					dim.SetBlock(pos.ShiftVertical(y), cactus);
				}
				return true;
			}
			return false;
		}

		private bool Probability(float prob)
		{
			return random.NextDouble() <= prob;
		}

		private bool Check(Dimension dim, BlockCoord pos, BlockID block)
		{
			var b = dim.GetBlock(pos);
			return b == block;
		}

		private bool Check(Dimension dim, BlockCoord pos, BlockID block1, BlockID block2)
		{
			var b = dim.GetBlock(pos);
			return b == block1 || b == block2;
		}

		private bool IsObstructed(Dimension dim, int x1, int y1, int z1, int x2, int y2, int z2)
		{
			for(int y = y1; y <= y2; y++)
			{
				for(int z = z1; z <= z2; z++)
				{
					for(int x = x1; x <= x2; x++)
					{
						if(!dim.IsAirOrNull((x, y, z))) return false;
					}
				}
			}
			return true;
		}

		private GeneratorType GetBiomeType(Dimension dim, BlockCoord pos)
		{
			var biome = dim.GetBiome(pos) ?? dim.DefaultBiome ?? BiomeID.Plains;
			switch(biome.id)
			{
				case "plains":
				case "swamp":
				case "swamp_hills":
				case "meadow":
					return GeneratorType.Plains;
				case "forest":
				case "wooded_hills":
				case "dark_forest":
				case "dark_forest_hills":
				case "flower_forest":
				case "grove":
					return GeneratorType.Forest;
				case "mountains":
				case "mountain_edge":
				case "gravelly_mountains":
				case "tundra":
				case "snowy_plains":
				case "snowy_slopes":
				case "windswept_gravelly_hills":
					return GeneratorType.ColdPlains;
				case "taiga":
				case "taiga_hills":
				case "snowy_taiga":
				case "snowy_taiga_hills":
				case "snowy_taiga_mountains":
				case "snowy_tundra":
					return GeneratorType.SpruceForest;
				case "birch_forest":
				case "birch_forest_hills":
				case "tall_birch_forest":
				case "tall_birch_hills":
				case "old_growth_birch_forest":
					return GeneratorType.BirchForest;
				case "desert":
				case "desert_hills":
				case "badlands":
				case "desert_lakes":
					return GeneratorType.Desert;
				case "the_void":
				case "ocean":
				case "deep_ocean":
				case "frozen_ocean":
				case "warm_ocean":
				case "lukewarm_ocean":
				case "cold_ocean":
				case "deep_warm_ocean":
				case "deep_lukewarm_ocean":
				case "deep_cold_ocean":
				case "deep_frozen_ocean":
				case "river":
				case "frozen_river":
				case "beach":
				case "stone_shore":
				case "snowy_beach":
				case "nether_wastes":
				case "soul_sand_valley":
				case "crimson_forest":
				case "warped_forest":
				case "basalt_deltas":
				case "the_end":
				case "end_barrens":
				case "end_highlands":
				case "end_midlands":
				case "small_end_islands":
					return GeneratorType.None;
				default:
					return GeneratorType.GrassOnly;
			}
		}
	}
}
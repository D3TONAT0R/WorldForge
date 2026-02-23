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
		private const int birchTrunkMinHeight = 2;
		private const int birchTrunkMaxHeight = 4;
		private const int spruceTrunkMinHeight = 1;
		private const int spruceTrunkMaxHeight = 4;

		private static readonly int[,,] blueprintOakTreeTop = new int[,,] {
		//YZX
		{
			{2,1,1,1,2},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{2,1,1,1,2}
		},{
			{2,1,1,1,2},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{2,1,1,1,2}
		},{
			{_,_,_,_,_},
			{_,2,1,2,_},
			{_,1,0,1,_},
			{_,2,1,2,_},
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

		private static readonly StructurePalette<Schematic.Block> oakTreePalette = new StructurePalette<Schematic.Block>(
			new Schematic.Block(new BlockState("oak_log")),
			new Schematic.Block(new BlockState("oak_leaves")),
			new Schematic.Block(new BlockState("oak_leaves"), probability: 0.5f)
		);
		private readonly Schematic oakTree = Schematic.From3DArray(oakTreePalette, blueprintOakTreeTop, true, new BlockCoord(2, 0, 2), 0, oakTrunkMinHeight, oakTrunkMaxHeight);

		private static readonly StructurePalette<Schematic.Block> birchTreePalette = new StructurePalette<Schematic.Block>(
			new Schematic.Block(new BlockState("birch_log")),
			new Schematic.Block(new BlockState("birch_leaves")),
			new Schematic.Block(new BlockState("oak_leaves"), probability: 0.5f)
		);
		private readonly Schematic birchTree = Schematic.From3DArray(birchTreePalette, blueprintOakTreeTop, true, new BlockCoord(2, 0, 2), 0, birchTrunkMinHeight, birchTrunkMaxHeight);

		private static readonly StructurePalette<Schematic.Block> spruceTreePalette = new StructurePalette<Schematic.Block>(
			new Schematic.Block(new BlockState("spruce_log")),
			new Schematic.Block(new BlockState("spruce_leaves"))
		);
		private readonly Schematic spruceTree = Schematic.From3DArray(spruceTreePalette, blueprintSpruceTree, true, new BlockCoord(2, 0, 2), 0, spruceTrunkMinHeight, spruceTrunkMaxHeight);

		public GeneratorType FixedGeneratorType { get; set; } = GeneratorType.None;

		public float GrassDensity { get; set; } = 0.15f;
		public float GeneralTreesPerChunk { get; set; } = 0.03f;
		public float ForestTreesPerChunk { get; set; } = 4f;
		public float ForestBirchRatio { get; set; } = 0.08f;
		public float CactiPerChunk { get; set; } = 0.25f;
		public float DeadBushPerChunk { get; set; } = 0.3f;

		private readonly BlockState grassBlock = new BlockState("grass_block");
		private readonly BlockState dirtBlock = new BlockState("dirt");
		private readonly BlockState sandBlock = new BlockState("sand");
		private readonly BlockState grass = new BlockState("short_grass");
		private readonly BlockState deadBush = new BlockState("dead_bush");
		private readonly BlockState cactus = new BlockState("cactus");

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public override bool UseMultithreading => false;

		public VegetationGenerator()
		{

		}

		public VegetationGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			GrassDensity = float.Parse(xml.Element("grass")?.Value ?? "0.2");
			GeneralTreesPerChunk = float.Parse(xml.Element("trees")?.Value ?? "0.3") / 128f;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			long seed = Seed;
			var gen = FixedGeneratorType == GeneratorType.None ? GetBiomeType(dimension, pos) : FixedGeneratorType;
			switch(gen)
			{
				case GeneratorType.None:
					break;
				case GeneratorType.GrassOnly:
					TrySpawnGrass(dimension, seed, pos);
					break;
				case GeneratorType.Plains:

					if(!TrySpawnTree(dimension, seed, pos, oakTree, GeneralTreesPerChunk))
					{
						TrySpawnGrass(dimension, seed + 1, pos);
					}
					break;
				case GeneratorType.Desert:
					if(!TrySpawnCactus(dimension, seed, pos, CactiPerChunk))
					{
						TrySpawnDeadBush(dimension, seed + 1, pos, DeadBushPerChunk * CHUNK_MULTIPLIER);
					}
					break;
				case GeneratorType.ColdPlains:
					if(!TrySpawnTree(dimension, seed, pos, oakTree, GeneralTreesPerChunk))
					{
						TrySpawnGrass(dimension, seed + 1, pos);
					}
					break;
				case GeneratorType.Forest:
					var tree = SeededRandom.Probability(ForestBirchRatio, seed + 123, pos) ? birchTree : oakTree;
					if(!TrySpawnTree(dimension, seed, pos, tree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, seed + 1, pos, GrassDensity * 0.5f);
					}
					break;
				case GeneratorType.BirchForest:
					if(!TrySpawnTree(dimension, seed, pos, birchTree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, seed + 1, pos, GrassDensity * 0.5f);
					}
					break;
				case GeneratorType.SpruceForest:
					if(!TrySpawnTree(dimension, seed, pos, spruceTree, ForestTreesPerChunk))
					{
						TrySpawnGrass(dimension, seed + 1, pos, GrassDensity * 0.5f);
					}
					break;
			}
		}

		private void TrySpawnGrass(Dimension dim, long seed, BlockCoord ground, float? probability = null)
		{
			if(SeededRandom.Probability(probability ?? GrassDensity, seed, ground)) PlaceGrass(dim, ground.Above, grass);
		}

		private void TrySpawnDeadBush(Dimension dim, long seed, BlockCoord ground, float probability)
		{
			if(SeededRandom.Probability(probability, seed, ground)) PlaceDeadBush(dim, ground.Above);
		}

		private bool TrySpawnTree(Dimension dim, long seed, BlockCoord ground, Schematic tree, float amountPerChunk)
		{
			if(tree == null) return false;
			if(SeededRandom.Probability(amountPerChunk * CHUNK_MULTIPLIER, seed, ground))
			{
				if(PlaceTree(dim, ground.Above, tree, seed + 791))
				{
					return true;
				}
			}
			return false;
		}

		private bool TrySpawnCactus(Dimension dim, long seed, BlockCoord ground, float amountPerChunk)
		{
			if(SeededRandom.Probability(amountPerChunk * CHUNK_MULTIPLIER, seed, ground))
			{
				return PlaceCactus(dim, ground.Above);
			}
			return false;
		}

		private bool PlaceTree(Dimension dim, BlockCoord pos, Schematic tree, long seed)
		{
			if(!Check(dim, pos.Below, grassBlock, dirtBlock) || !dim.IsAirOrNull(pos.Above)) return false;
			//if(IsObstructed(region, x, y+1, z, x, y+bareTrunkHeight, z) || IsObstructed(region, x-w, y+bareTrunkHeight, z-w, x+w, y+bareTrunkHeight+treeTopHeight, z+w)) return false;
			dim.SetBlock(pos.Below, "minecraft:dirt");
			tree.Build(dim, pos, seed, false);
			return true;
		}

		private bool PlaceGrass(Dimension dim, BlockCoord pos, BlockState block)
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
			if(Check(dim, pos.Below, sandBlock) && dim.IsAirOrNull(pos.Above) && CountFreeNeighbors(dim, pos) == 4)
			{
				int height = random.Value.Next(1, 4);
				for(int y = 0; y < height; y++)
				{
					dim.SetBlock(pos.ShiftVertical(y), cactus);
				}
				return true;
			}
			return false;
		}

		private bool Check(Dimension dim, BlockCoord pos, BlockState block)
		{
			var b = dim.GetBlock(pos);
			return b == block.Block;
		}

		private int CountFreeNeighbors(Dimension dim, BlockCoord pos)
		{
			int free = 0;
			if(dim.IsAirOrNull(pos.North)) free++;
			if(dim.IsAirOrNull(pos.South)) free++;
			if(dim.IsAirOrNull(pos.East)) free++;
			if(dim.IsAirOrNull(pos.West)) free++;
			return free;
		}

		private bool Check(Dimension dim, BlockCoord pos, BlockState block1, BlockState block2)
		{
			var b = dim.GetBlock(pos);
			return b == block1.Block || b == block2.Block;
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
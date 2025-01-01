using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Structures;

namespace WorldForge.Builders.PostProcessors
{
	public class VegetationPostProcessor : PostProcessor
	{
		private const int z = -1;

		private const int oakTrunkMinHeight = 1;
		private const int oakTrunkMaxHeight = 3;

		private static readonly int[,,] blueprintOakTreeTop = new int[,,] {
		//YZX
		{
			{z,z,z,z,z},
			{z,z,1,z,z},
			{z,1,0,1,z},
			{z,z,1,z,z},
			{z,z,z,z,z},
		},{
			{z,1,1,1,z},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{z,1,1,1,z}
		},{
			{z,1,1,1,z},
			{1,1,1,1,1},
			{1,1,0,1,1},
			{1,1,1,1,1},
			{z,1,1,1,z}
		},{
			{z,z,z,z,z},
			{z,1,1,1,z},
			{z,1,0,1,z},
			{z,1,1,1,z},
			{z,z,z,z,z}
		},{
			{z,z,z,z,z},
			{z,z,1,z,z},
			{z,1,1,1,z},
			{z,z,1,z,z},
			{z,z,z,z,z}
			}
		};
		private static readonly StructurePalette<WFStructure.Block> oakTreePalette = new StructurePalette<WFStructure.Block>(
			new WFStructure.Block(new BlockState("oak_log")),
			new WFStructure.Block(new BlockState("oak_leaves"))
		);
		private readonly WFStructure oakTree = WFStructure.From3DArray(oakTreePalette, blueprintOakTreeTop, true, new BlockCoord(2, 0, 2), 0, oakTrunkMinHeight, oakTrunkMaxHeight);

		private float grassDensity;
		private float treesDensity;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public VegetationPostProcessor(float grassPerBlock = 0.2f, float treesPerChunk = 0.3f)
		{
			grassDensity = grassPerBlock;
			treesDensity = treesPerChunk / 128f;
		}

		public VegetationPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			grassDensity = float.Parse(xml.Element("grass")?.Value ?? "0.2");
			treesDensity = float.Parse(xml.Element("trees")?.Value ?? "0.3") / 128f;
		}

		protected override void OnProcessSurface(Dimension world, BlockCoord pos, int pass, float mask)
		{
			//Place trees
			if(random.NextDouble() <= treesDensity)
			{
				if(PlaceTree(world, pos.Above))
				{
					//A tree was placed, there is nothing left to do here
					return;
				}
			}
			//Place tall grass
			if(random.NextDouble() <= grassDensity)
			{
				PlaceGrass(world, pos.Above);
			}
		}

		private bool PlaceTree(Dimension dim, BlockCoord pos)
		{
			var b = dim.GetBlock(pos.Below);
			if(b == null || !CanGrowPlant(b)) return false;
			if(!dim.IsAirOrNull(pos.Above)) return false;
			//if(IsObstructed(region, x, y+1, z, x, y+bareTrunkHeight, z) || IsObstructed(region, x-w, y+bareTrunkHeight, z-w, x+w, y+bareTrunkHeight+treeTopHeight, z+w)) return false;
			dim.SetBlock(pos.Below, "minecraft:dirt");
			oakTree.Build(dim, pos, random);
			return true;
		}

		private bool PlaceGrass(Dimension dim, BlockCoord pos)
		{
			var b = dim.GetBlock(pos.Below);
			if(b == null || b.ID != "minecraft:grass_block") return false;
			return dim.SetBlock(pos, "minecraft:grass");
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

		private bool CanGrowPlant(BlockID block)
		{
			return block.ID == "minecraft:grass_block" || block.ID == "minecraft:dirt";
		}
	}
}
using System.Xml.Linq;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class NaturalSurfaceGenerator : PostProcessor
	{
		public enum SurfaceType
		{
			BiomeBased = 0,
			Grass = 1,
			Sand = 2
		}

		public SurfaceType Surface { get; set; } = SurfaceType.BiomeBased;

		public int WaterLevel { get; set; } = int.MinValue;

		private readonly BlockState grassBlock = new BlockState("grass_block");
		private readonly BlockState dirtBlock = new BlockState("dirt");
		private readonly BlockState gravelBlock = new BlockState("gravel");
		private readonly BlockState sandBlock = new BlockState("sand");
		private readonly BlockState waterBlock = new BlockState("water");

		private readonly BiomeID desert = BiomeIDs.Get("desert");

		public override Priority OrderPriority => Priority.BeforeDefault;
		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public NaturalSurfaceGenerator(int waterLevel = -256, SurfaceType surface = SurfaceType.BiomeBased)
		{
			WaterLevel = waterLevel;
			Surface = surface;
		}

		public NaturalSurfaceGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			WaterLevel = int.Parse(xml.Element("waterlevel")?.Value ?? "-1");
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			//Place grass on top & 3 layers of dirt below
			if (pos.y > WaterLevel)
			{
				bool sand = Surface == SurfaceType.Sand || (Surface == SurfaceType.BiomeBased && dimension.GetBiome(pos) == desert);
				if (!sand)
				{
					dimension.SetBlock(pos, grassBlock);
					dimension.SetBlockColumn(pos.XZ, pos.y - 1, pos.y - 4, dirtBlock);
				}
				else
				{
					dimension.SetBlockColumn(pos.XZ, pos.y, pos.y - 4, sandBlock);
				}
			}
			else
			{
				dimension.SetBlockColumn(pos.XZ, pos.y, pos.y - 4, gravelBlock);
			}
			//Fill the terrain with water up to the waterLevel
			if (WaterLevel != int.MinValue && pos.y <= WaterLevel)
			{
				for (int y = pos.y; y <= WaterLevel; y++)
				{
					var newPos = new BlockCoord(pos.x, y, pos.z);
					if (dimension.IsAirOrNull(newPos)) dimension.SetBlock(newPos, waterBlock);
				}
			}
		}
	}
}
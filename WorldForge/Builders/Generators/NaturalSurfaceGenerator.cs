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

		public NaturalSurfaceGenerator(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			WaterLevel = int.Parse(xml.Element("waterlevel")?.Value ?? "-1");
		}

		public NaturalSurfaceGenerator(int waterLevel = -256, SurfaceType surface = SurfaceType.BiomeBased)
		{
			WaterLevel = waterLevel;
			Surface = surface;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			//Place grass on top & 3 layers of dirt below
			if(pos.y > WaterLevel)
			{
				bool sand = Surface == SurfaceType.Sand || (Surface == SurfaceType.BiomeBased && dimension.GetBiome(pos) == desert);
				if(!sand)
				{
					dimension.SetBlock(pos, grassBlock);
					for(int i = 1; i <= 3; i++)
					{
						dimension.SetBlock((pos.x, pos.y - i, pos.z), dirtBlock);
					}
				}
				else
				{
					for(int i = 0; i <= 4; i++)
					{
						dimension.SetBlock((pos.x, pos.y - i, pos.z), sandBlock);
					}
				}
			}
			else
			{
				for(int i = 0; i < 4; i++)
				{
					dimension.SetBlock((pos.x, pos.y - i, pos.z), gravelBlock);
				}
			}
			//Fill the terrain with water up to the waterLevel
			if(WaterLevel != int.MinValue && pos.y <= WaterLevel)
			{
				for(int y = pos.y; y <= WaterLevel; y++)
				{
					var newPos = new BlockCoord(pos.x, y, pos.z);
					if(dimension.IsAirOrNull(newPos)) dimension.SetBlock(newPos, waterBlock);
				}
			}
		}
	}
}
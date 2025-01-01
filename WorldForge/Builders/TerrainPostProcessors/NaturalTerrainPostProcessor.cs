using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class NaturalTerrainPostProcessor : PostProcessor
	{
		public int waterLevel = int.MinValue;

		private BlockState grassBlock = new BlockState("minecraft:grass_block");
		private BlockState dirtBlock = new BlockState("minecraft:dirt");
		private BlockState gravelBlock = new BlockState("minecraft:gravel");
		private BlockState waterBlock = new BlockState("minecraft:water");

		public override Priority OrderPriority => Priority.BeforeDefault;
		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public NaturalTerrainPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			waterLevel = int.Parse(xml.Element("waterlevel")?.Value ?? "-1");
		}

		public NaturalTerrainPostProcessor(int waterLevel = -256)
		{
			this.waterLevel = waterLevel;
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			//Place grass on top & 3 layers of dirt below
			if(pos.y > waterLevel + 1)
			{
				dim.SetBlock(pos, grassBlock);
				for(int i = 1; i < 4; i++)
				{
					dim.SetBlock((pos.x, pos.y - i, pos.z), dirtBlock);
				}
			}
			else
			{
				for(int i = 0; i < 4; i++)
				{
					dim.SetBlock((pos.x, pos.y - i, pos.z), gravelBlock);
				}
			}
			//Fill the terrain with water up to the waterLevel
			if(waterLevel != int.MinValue && pos.y <= waterLevel)
			{
				for(int y = pos.y; y <= waterLevel; y++)
				{
					var newPos = new BlockCoord(pos.x, y, pos.z);
					if(dim.IsAirOrNull(newPos)) dim.SetBlock(newPos, waterBlock);
				}
			}
		}
	}
}
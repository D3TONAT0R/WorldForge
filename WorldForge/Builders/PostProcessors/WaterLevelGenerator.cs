using System;
using System.IO;
using System.Xml.Linq;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class WaterLevelGenerator : PostProcessor
	{

		int waterLevel = 62;
		public string waterBlock = "minecraft:water";
		Heightmap waterSurfaceMap;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WaterLevelGenerator(int level = 62)
		{
			waterLevel = level;
		}

		public WaterLevelGenerator(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			worldOriginOffsetX = offsetX;
			worldOriginOffsetZ = offsetZ;
			var fileXml = xml.Element("file");
			if(fileXml != null)
			{
				string path = Path.Combine(rootPath, xml.Element("file").Value);
				waterSurfaceMap = Heightmap.FromImage(path, 0, 0, sizeX, sizeZ);
			}
			xml.TryParseInt("waterlevel", ref waterLevel);
			if(xml.Element("waterblock") != null) waterBlock = xml.Element("waterblock").Value;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			int start = waterLevel;
			if(waterSurfaceMap != null)
			{
				start = Math.Max(waterSurfaceMap?.heights[pos.x - worldOriginOffsetX, pos.z - worldOriginOffsetZ] ?? (short)-1, waterLevel);
			}
			for(int y2 = start; y2 > pos.y; y2--)
			{
				BlockCoord pos2 = (pos.x, y2, pos.z);
				if(dimension.IsAirOrNull(pos2))
				{
					dimension.SetBlock(pos2, waterBlock);
				}
			}
		}
	}
}
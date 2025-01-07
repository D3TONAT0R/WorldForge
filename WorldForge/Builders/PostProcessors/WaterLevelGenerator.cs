using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class WaterLevelGenerator : PostProcessor
	{

		int waterLevel = 62;
		public string waterBlock = "minecraft:water";
		Map<byte> waterSurfaceMap;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WaterLevelGenerator(int level = 62)
		{
			waterLevel = level;
		}

		public WaterLevelGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			var fileXml = xml.Element("file");
			if(fileXml != null)
			{
				string path = Path.Combine(rootPath, fileXml.Value);
				waterSurfaceMap = Map<byte>.CreateByteMap(path);
				if(fileXml.TryGetAttribute("origin", out var origin))
				{
					waterSurfaceMap.LowerCornerPos = BlockCoord2D.Parse(origin.Value);
				}
			}
			xml.TryParseInt("waterlevel", ref waterLevel);
			if(xml.Element("waterblock") != null) waterBlock = xml.Element("waterblock").Value;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			int start = waterLevel;
			if(waterSurfaceMap != null)
			{
				short h = -1;
				if(waterSurfaceMap.TryGetValue(pos, 0, out var hb))
				{
					h = hb;
				}
				start = Math.Max(h, waterLevel);
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
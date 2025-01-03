using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class WeightmappedTerrainGenerator : PostProcessor
	{

		public Weightmap<byte> map;
		public List<SurfaceLayer> layers = new List<SurfaceLayer>();

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WeightmappedTerrainGenerator()
		{

		}

		public WeightmappedTerrainGenerator(XElement xml, string rootPath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
			: base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			string mapFileName = Path.Combine(rootPath, xml.Attribute("file").Value);
			foreach(var layer in xml.Elements("layer"))
			{
				XAttribute colorAttr = layer.Attribute("color");
				if(colorAttr == null)
				{
					throw new ArgumentException("layer is missing required attribute 'color': " + layer.ToString().Trim());
				}
				var color = ParseColor(colorAttr.Value);
				var surfaceLayer = new SurfaceLayer(color, layer.Attribute("name")?.Value);
				layers.Add(surfaceLayer);
				foreach(var elem in layer.Elements())
				{
					if(elem.Name.LocalName == "surface")
					{
						surfaceLayer.AddSurfaceGenerator(elem);
					}
					else if(elem.Name.LocalName == "gen")
					{
						surfaceLayer.AddSchematicGenerator(this, elem);
					}
					else if(elem.Name.LocalName == "biome")
					{
						surfaceLayer.AddBiomeGenerator(elem);
					}
				}
			}

			Rgba32[] mappedColors = new Rgba32[layers.Count];
			for(int i = 0; i < layers.Count; i++)
			{
				mappedColors[i] = layers[i].layerColor;
			}

			map = Weightmap<byte>.GetFixedWeightmap(mapFileName, mappedColors, ditherLimit, 0, 0, sizeX, sizeZ);
		}

		Rgba32 ParseColor(string input)
		{
			Rgba32 c;
			if(input.Contains(","))
			{
				//It's a manually defined color
				string[] cs = input.Split(',');
				var r = byte.Parse(cs[0]);
				var g = byte.Parse(cs[1]);
				var b = byte.Parse(cs[2]);
				c = new Rgba32(r, g, b);
			}
			else
			{
				c = CommonWeightmapColors.NameToColor(input);
			}
			return c;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord topPos, int pass, float mask)
		{
			byte i = map.GetValue(topPos.x - worldOriginOffsetX, topPos.z - worldOriginOffsetZ);
			if(i < 255)
			{
				layers[i].RunGenerator(dimension, topPos, Seed);
			}
		}
	}
}

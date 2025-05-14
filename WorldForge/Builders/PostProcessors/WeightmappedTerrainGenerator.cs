using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class WeightmappedTerrainGenerator : PostProcessor
	{
		public class WeightmapChannel
		{
			public string name;
			public BitmapColor layerColor;
			public PostProcessingChain chain = new PostProcessingChain();

			public WeightmapChannel(BitmapColor color, string name = null)
			{
				layerColor = color;
				this.name = name;
			}
		}


		public Map<byte> map;
		public List<WeightmapChannel> channels = new List<WeightmapChannel>();

		public override PostProcessType PostProcessorType => PostProcessType.Surface | PostProcessType.Block;

		public WeightmappedTerrainGenerator()
		{

		}

		public WeightmappedTerrainGenerator(XElement xml, string rootPath)
			: base(rootPath, xml)
		{
			string mapFileName = Path.Combine(rootPath, xml.Attribute("file").Value);
			int ditherLimit = int.Parse(xml.Attribute("ditherLimit")?.Value ?? "0");
			foreach(var layer in xml.Elements("layer"))
			{
				XAttribute colorAttr = layer.Attribute("color");
				if(colorAttr == null)
				{
					throw new ArgumentException("layer is missing required attribute 'color': " + layer.ToString().Trim());
				}
				var color = ParseColor(colorAttr.Value);
				var channel = new WeightmapChannel(color, layer.Attribute("name")?.Value);
				channels.Add(channel);
				channel.chain.LoadSettings(rootPath, layer);
			}

			BitmapColor[] mappedColors = new BitmapColor[channels.Count];
			for(int i = 0; i < channels.Count; i++)
			{
				mappedColors[i] = channels[i].layerColor;
			}

			map = Map<byte>.CreateFixedMap(mapFileName, mappedColors, ditherLimit);
			if(xml.TryGetElement("origin", out var origin))
			{
				map.LowerCornerPos = BlockCoord2D.Parse(origin.Value);
			}
		}

		BitmapColor ParseColor(string input)
		{
			BitmapColor c;
			if(input.Contains(","))
			{
				//It's a manually defined color
				string[] cs = input.Split(',');
				var r = byte.Parse(cs[0]);
				var g = byte.Parse(cs[1]);
				var b = byte.Parse(cs[2]);
				c = new BitmapColor(r, g, b);
			}
			else
			{
				c = CommonWeightmapColors.NameToColor(input);
			}
			return c;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord topPos, int pass, float mask)
		{
			byte i = map.GetValueOrDefault(topPos.XZ, 0, 255);
			if(i < 255)
			{
				var chunk = dimension.GetRegionAtBlock(topPos).GetChunkAtBlock(topPos, false);
				var x = topPos.x & 15;
				var z = topPos.z & 15;
				var bx = topPos.x - x;
				var bz = topPos.z - z;
				foreach(var gen in channels[i].chain.processors)
				{
					gen.Context = Context;
					gen.TryProcessSurface(chunk, bx, x, bz, z, pass);
				}
			}
		}

		protected override void OnProcessBlock(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			byte i = map.GetValueOrDefault(pos.XZ, 0, 255);
			if(i < 255)
			{
				var chunk = dimension.GetRegionAtBlock(pos).GetChunkAtBlock(pos, false);
				var x = pos.x & 15;
				var z = pos.z & 15;
				var bx = pos.x - x;
				var bz = pos.z - z;
				foreach(var gen in channels[i].chain.processors)
				{
					gen.Context = Context;
					gen.TryProcessBlock(chunk, bx, x, bz, z, pass);
				}
			}
		}
	}
}

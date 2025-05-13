using SimpleNoise;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class SurfaceBlocksGenerator : PostProcessor
	{
		public List<BlockID> blocks = new List<BlockID>();

		public NoiseParameters noiseParameters;
		public float noiseThreshold = 0;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public SurfaceBlocksGenerator(IEnumerable<BlockID> blockLayer)
		{
			blocks.AddRange(blockLayer);
		}

		public SurfaceBlocksGenerator(params BlockID[] blockLayer) : this((IEnumerable<BlockID>)blockLayer)
		{

		}

		public SurfaceBlocksGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			blocks = xml.Attribute("blocks").Value.Split(',').Select(v => BlockList.Find(v)).ToList();
			var noise = xml.TryGetElement("noise", out var noiseElem);
			if(noise)
			{
				var scale = float.Parse(noiseElem.Attribute("scale")?.Value ?? "1.0");
				var octaves = int.Parse(noiseElem.Attribute("octaves")?.Value ?? "1");
				var persistence = float.Parse(noiseElem.Attribute("persistence")?.Value ?? "0.5");
				var lacunarity = float.Parse(noiseElem.Attribute("lacunarity")?.Value ?? "2.0");
				var fractalParams = new FractalParameters(octaves, persistence, lacunarity);
				noiseParameters = new NoiseParameters(scale, fractalParameters: fractalParams);
				noiseThreshold = float.Parse(noiseElem.Attribute("threshold")?.Value ?? "0.5");
			}
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			if(noiseThreshold > 0)
			{
				if(PerlinNoise.Instance.GetNoise2D(new System.Numerics.Vector2(pos.x * 24.6f, pos.z * 24.6f)) > noiseThreshold) return;
			}
			for(int i = 0; i < blocks.Count; i++)
			{
				dimension.SetBlock(pos.ShiftVertical(-i), blocks[i]);
			}
		}
	}
}
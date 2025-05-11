using System;
using System.Xml.Linq;
using WorldForge.Biomes;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class BiomeGenerator : PostProcessor
	{
		private BiomeID biomeID;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public BiomeGenerator(BiomeID biome)
		{
			biomeID = biome;
		}

		public BiomeGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			var id = xml.Attribute("id");
			if(id != null && id.Value.Length > 0)
			{
				if(char.IsDigit(id.Value[0]))
				{
					biomeID = BiomeIDs.GetFromNumeric(byte.Parse(id.Value));
				}
				else
				{
					biomeID = (BiomeID)Enum.Parse(typeof(BiomeID), id.Value);
				}
			}
			else
			{
				throw new ArgumentException("Biome generator is missing 'id' attribute");
			}
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			dimension.SetBiome(pos.x, pos.z, biomeID);
		}
	}
}
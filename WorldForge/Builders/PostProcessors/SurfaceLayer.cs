using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SimpleNoise;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using WorldForge.Structures;

namespace WorldForge.Builders.PostProcessors
{
	public abstract class SurfaceLayerGenerator
	{

		public int yMin = int.MinValue;
		public int yMax = int.MaxValue;

		public abstract bool Generate(Dimension dim, BlockCoord pos, long seed);

		protected bool SetBlock(Dimension dim, BlockCoord pos, string b)
		{
			if(!string.IsNullOrWhiteSpace(b) && !dim.IsAirOrNull(pos))
			{
				return dim.SetBlock(pos, b);
			}
			else
			{
				return false;
			}
		}
	}

	public class StandardSurfaceLayerGenerator : SurfaceLayerGenerator
	{
		public List<string> blocks = new List<string>();

		public StandardSurfaceLayerGenerator(IEnumerable<string> blockLayer)
		{
			blocks.AddRange(blockLayer);
		}

		public StandardSurfaceLayerGenerator(params string[] blockLayer) : this((IEnumerable<string>)blockLayer)
		{

		}

		public override bool Generate(Dimension dim, BlockCoord pos, long seed)
		{
			if(pos.y < yMin || pos.y > yMax)
			{
				return false;
			}
			bool b = false;
			for(int i = 0; i < blocks.Count; i++)
			{
				b |= SetBlock(dim, (pos.x, pos.y - i, pos.z), blocks[i]);
			}
			return b;
		}
	}

	public class PerlinSurfaceLayerGenerator : StandardSurfaceLayerGenerator
	{
		public NoiseParameters parameters;

		public float threshold;

		public PerlinSurfaceLayerGenerator(IEnumerable<string> blockLayer, float scale, float threshold) : base(blockLayer)
		{
			var noiseScale = 1f / scale * 24.6f;
			parameters = new NoiseParameters(noiseScale);
			this.threshold = threshold;
		}

		public override bool Generate(Dimension dim, BlockCoord pos, long seed)
		{
			if(PerlinNoise.Instance.GetNoise2D(new System.Numerics.Vector2(pos.x, pos.z)) < threshold)
			{
				return base.Generate(dim, pos, seed);
			}
			else
			{
				return false;
			}
		}
	}

	public class SchematicInstanceGenerator : SurfaceLayerGenerator
	{

		private float chance;
		private Schematic schematic;
		private string block;
		private bool isPlant;

		public SchematicInstanceGenerator(Schematic schem, float chance, bool doPlantCheck)
		{
			this.chance = chance;
			schematic = schem;
			isPlant = doPlantCheck;
		}

		public SchematicInstanceGenerator(string blockID, float chance, bool doPlantCheck)
		{
			this.chance = chance;
			block = blockID;
			isPlant = doPlantCheck;
		}

		public override bool Generate(Dimension dim, BlockCoord pos, long seed)
		{
			if(isPlant && (!Blocks.IsPlantSustaining(dim.GetBlock(pos)) || !dim.IsAirOrNull(pos.Above))) return false;
			if(pos.y < yMin || pos.y > yMax) return false;

			if(SeededRandom.Probability(chance / 128f, seed, pos))
			{
				if(schematic != null)
				{
					schematic.Build(dim, pos.Above, seed, false);
					return true;
				}
				else
				{
					return dim.SetBlock(pos.Above, block);
				}
			}
			else
			{
				return false;
			}
		}
	}

	public class BiomeGenerator : SurfaceLayerGenerator
	{
		private BiomeID biomeID;

		public BiomeGenerator(BiomeID biome)
		{
			biomeID = biome;
		}

		public override bool Generate(Dimension dim, BlockCoord pos, long seed)
		{
			if(pos.y < yMin || pos.y > yMax) return false;
			dim.SetBiome(pos.x, pos.z, biomeID);
			return true;
		}
	}


	public class SurfaceLayer
	{

		public string name;
		public BitmapColor layerColor;
		public List<SurfaceLayerGenerator> generators = new List<SurfaceLayerGenerator>();

		public SurfaceLayer(BitmapColor color, string name = null)
		{
			layerColor = color;
			this.name = name;
		}

		public bool AddSurfaceGenerator(XElement xml, bool throwExceptions = true)
		{
			string type = xml.Attribute("type")?.Value ?? "standard";
			string[] blocks = xml.Attribute("blocks").Value.Split(',');
			SurfaceLayerGenerator gen = null;
			if(type == "standard" || string.IsNullOrWhiteSpace(type))
			{
				gen = new StandardSurfaceLayerGenerator(blocks);
			}
			else if(type == "perlin")
			{
				float scale = float.Parse(xml.Attribute("scale")?.Value ?? "1.0");
				float threshold = float.Parse(xml.Attribute("threshold")?.Value ?? "0.5");
				gen = new PerlinSurfaceLayerGenerator(blocks, scale, threshold);
			}

			if(gen != null)
			{
				if(xml.Attribute("y-min") != null)
				{
					gen.yMin = int.Parse(xml.Attribute("y-min").Value);
				}
				if(xml.Attribute("y-max") != null)
				{
					gen.yMax = int.Parse(xml.Attribute("y-max").Value);
				}
				generators.Add(gen);
				return true;
			}
			else
			{
				if(throwExceptions) throw new ArgumentException("Unknown generator type: " + type);
				return false;
			}
		}

		public bool AddSchematicGenerator(WeightmappedTerrainGenerator gen, XElement xml, bool throwExceptions = true)
		{
			var schem = xml.Attribute("schem");
			var amount = 1f;
			xml.TryParseFloatAttribute("amount", ref amount);
			bool plantCheck = true;
			xml.TryParseBoolAttribute("plant-check", ref plantCheck);
			if(schem != null)
			{
				if(gen.Context.Schematics.TryGet(schem.Value, out var schematic))
				{
					generators.Add(new SchematicInstanceGenerator(schematic, amount, plantCheck));
				}
				else
				{
					if(throwExceptions) throw new ArgumentException($"Could not find schematic named '{schem.Value}'");
					return false;
				}
				return true;
			}
			else
			{
				var block = xml.Attribute("block");
				if(block != null)
				{
					generators.Add(new SchematicInstanceGenerator(block.Value, amount, plantCheck));
					return true;
				}
				else
				{
					if(throwExceptions) throw new ArgumentException("block/schematic generator has missing arguments (must have either 'block' or 'schem')");
					return false;
				}
			}
		}

		public bool AddBiomeGenerator(XElement xml, bool throwExceptions = true)
		{
			var id = xml.Attribute("id");
			if(id != null && id.Value.Length > 0)
			{
				if(char.IsDigit(id.Value[0]))
				{
					generators.Add(new BiomeGenerator(BiomeIDs.GetFromNumeric(byte.Parse(id.Value))));
				}
				else
				{
					generators.Add(new BiomeGenerator((BiomeID)Enum.Parse(typeof(BiomeID), id.Value)));
				}
				return true;
			}
			else
			{
				if(throwExceptions) throw new ArgumentException("Biome generator is missing 'id' attribute");
				return false;
			}
		}

		public void RunGenerator(Dimension dim, BlockCoord pos, long seed)
		{
			for(int i = 0; i < generators.Count; i++)
			{
				generators[i].Generate(dim, pos, seed + 41 + i * 37);
			}
		}
	}
}

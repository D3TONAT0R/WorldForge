using System;
using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class OreGenerator : LayeredGenerator
	{

		public class OreGenLayer : Layer
		{
			public List<OreVeinGenerator> ores = new List<OreVeinGenerator>();
			public float multiplier = 1;
			public bool useVanilla = false;

			public override void ProcessBlockColumn(Dimension dim, Random random, BlockCoord topPos, float mask)
			{
				float spawnChanceMultiplier = multiplier * mask;
				foreach(var ore in ores)
				{
					ore.Generate(dim, random, spawnChanceMultiplier, topPos.x, topPos.z);
				}
				if(useVanilla)
				{
					foreach(var ore in GetVanillaOreGenerators(dim.ParentWorld?.GameVersion ?? GameVersion.LastSupportedVersion))
					{
						ore.Generate(dim, random, spawnChanceMultiplier, topPos.x, topPos.z);
					}
				}
			}
		}

		public Map<float> map;
		public float rarityMul = 1;

		public static readonly List<OreVeinGenerator> vanillaOres_pre_1_17 = new List<OreVeinGenerator>()
		{
			new OreVeinGenerator("iron_ore", 9, 7.5f, 2, 64),
			new OreVeinGenerator("coal_ore", 24, 5.5f, 16, 120),
			new OreVeinGenerator("gold_ore", 9, 1f, 2, 30),
			new OreVeinGenerator("diamond_ore", 8, 0.35f, 2, 16),
			new OreVeinGenerator("redstone_ore", 10, 1.2f, 4, 16),
			new OreVeinGenerator("lapis_ore", 9, 0.7f, 4, 28)
		};

		public static readonly List<OreVeinGenerator> vanillaOres_1_17 = new List<OreVeinGenerator>(vanillaOres_pre_1_17)
		{
			new OreVeinGenerator("copper_ore", 10, 12.5f, 0, 72)
		};

		public static readonly List<OreVeinGenerator> vanillaOres_1_18 = new List<OreVeinGenerator>()
		{
			//TODO: make them match 1.18s non-uniform distribution pattern

			//Coal - linear
			new OreVeinGenerator("coal_ore", 16, 56, -2, 192, 1f, 96),
			//High coal - constant
			new OreVeinGenerator("coal_ore", 16, 28, 136, 256),

			//Iron - linear
			new OreVeinGenerator("iron_ore", 9, 13, -26, 54, 1f, 14),
			//Iron - constant
			new OreVeinGenerator("iron_ore", 9, 5, -64, 70),

			//Gold - linear
			new OreVeinGenerator("gold_ore", 9, 5.5f, -64, 30, 1f, -17),
			//Low gold - constant
			new OreVeinGenerator("gold_ore", 9, 0.25f, -64, -52),

			//Copper - linear
			new OreVeinGenerator("copper_ore", 11, 30, -16, 112, 1f, 48),

			//Diamond - linear
			new OreVeinGenerator("diamond_ore", 6, 12, -144, 16, 1f, -64),

			//Emerald - custom interpretation (not biome based)
			//new OreGenerator("emerald_ore", 3, 2, -16, 300, 1f, 240),

			//Lapis - linear
			new OreVeinGenerator("lapis_ore", 7, 2.8f, -32, 32, 1f, 0),
			//Lapis - constant
			new OreVeinGenerator("lapis_ore", 7, 2.8f, -64, 62),

			//Redstone - linear
			new OreVeinGenerator("redstone_ore", 9, 9f, -88, -32, 1f, -64),
			//Redstone - constant
			new OreVeinGenerator("redstone_ore", 9, 2.5f, -64, 12)
		};

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public OreGenerator(bool useVanillaOreGenerators)
		{
			if(useVanillaOreGenerators)
			{
				layers.Add(-1, new OreGenLayer
				{
					useVanilla = true
				});
			}
		}

		public OreGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			rarityMul = float.Parse(xml.Element("multiplier")?.Value ?? "1");
			var map = xml.Element("map");
			this.map = LoadWeightmap(rootPath, xml, out var weightXml);
			if(weightXml != null)
			{
				LoadLayers(weightXml.Elements(), xe => CreateLayer(xe));
			}
			if(this.map != null)
			{
				var gen = new OreGenLayer
				{
					useVanilla = true
				};
				layers.Add(-1, gen);
			}
		}

		public static List<OreVeinGenerator> GetVanillaOreGenerators(GameVersion gameVersion)
		{
			if(gameVersion >= GameVersion.Release_1(18)) return vanillaOres_1_18;
			else if(gameVersion >= GameVersion.Release_1(17)) return vanillaOres_1_17;
			else return vanillaOres_pre_1_17;
		}

		private Layer CreateLayer(XElement elem)
		{
			var layer = new OreGenLayer();
			foreach(var oreElem in elem.Elements())
			{
				var elemName = oreElem.Name.LocalName.ToLower();
				if(elemName == "gen")
				{
					layer.ores.Add(new OreVeinGenerator(oreElem));
				}
				else if(elemName == "default")
				{
					layer.useVanilla = true;
				}
				else if(elemName == "multiplier")
				{
					layer.multiplier = float.Parse(oreElem.Value);
				}
				else
				{
					throw new ArgumentException("Unexpected element name: " + elemName);
				}
			}
			return layer;
		}

		protected override void OnBegin()
		{
			
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord topPos, int pass, float mask)
		{
			//if (topPos.y < 4) return;
			ProcessWeightmapLayersSurface(layers, map, dimension, topPos, pass, mask);
		}
	}
}

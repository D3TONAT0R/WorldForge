using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SimpleNoise;
using WorldForge.Coordinates;
using SVector3 = System.Numerics.Vector3;

namespace WorldForge.Builders.PostProcessors
{
	public class CaveGenerator : LayeredGenerator
	{

		public abstract class Carver
		{
			public int lavaHeight = 8;

			public Carver(XElement elem)
			{

			}

			public abstract void ProcessBlockColumn(Dimension dim, BlockCoord topPos, float mask, Random random);

			protected bool CarveSphere(Dimension dim, SVector3 pos, float radius, bool breakSurface)
			{
				bool hasCarved = false;
				int x1 = (int)Math.Floor(pos.X - radius);
				int x2 = (int)Math.Ceiling(pos.X + radius);
				int y1 = (int)Math.Floor(pos.Y - radius);
				int y2 = (int)Math.Ceiling(pos.Y + radius);
				int z1 = (int)Math.Floor(pos.Z - radius);
				int z2 = (int)Math.Ceiling(pos.Z + radius);
				var radiusSq = radius * radius;
				for(int x = x1; x <= x2; x++)
				{
					for(int y = y1; y <= y2; y++)
					{
						for(int z = z1; z <= z2; z++)
						{
							if(DistanceSquared(new SVector3(x, y, z), pos) < radiusSq)
							{
								hasCarved |= CarveBlock(dim, (x, y, z), breakSurface);
							}
						}
					}
				}
				return hasCarved;
			}

			private float DistanceSquared(SVector3 a, SVector3 b)
			{
				return (a - b).LengthSquared();
			}

			protected bool CarveBlock(Dimension dim, BlockCoord pos, bool allowSurfaceBreak)
			{
				var b = dim.GetBlock(pos);

				if(b == null || Blocks.IsAir(b)) return false;

				//Can the cave break the surface?
				if(b.CompareMultiple(Blocks.terrainSurfaceBlocks) && !allowSurfaceBreak) return false;
				//Ignore bedrock and existing liquids
				if(b == Blocks.bedrock || Blocks.IsLiquid(b)) return false;

				if(pos.y <= lavaHeight)
				{
					dim.SetBlock(pos, BlockState.Lava);
				}
				else
				{
					dim.SetBlock(pos, BlockState.Air);
				}
				return true;
			}

			protected float RandomRange(float min, float max)
			{
				return min + (float)random.Value.NextDouble() * (max - min);
			}

			protected bool Probability(double prob)
			{
				return random.Value.NextDouble() <= prob;
			}
		}

		public class CaveCarver : Carver
		{
			public enum Distibution
			{
				Equal,
				FavorBottom,
				FavorTop
			}

			const double invChunkArea = 1f / 256f;

			public float amount = 1;
			public Distibution distibution = Distibution.FavorBottom;
			public int yMin = 8;
			public int yMax = 92;
			public float scale = 1f;
			public float variationScale = 1f;

			public CaveCarver(XElement elem) : base(elem)
			{
				if(elem != null)
				{
					elem.TryParseFloat("amount", ref amount);
					if(elem.TryGetElement("distribution", out var e))
					{
						string v = e.Value.ToLower();
						if(v == "equal") distibution = Distibution.Equal;
						else if(v == "bottom") distibution = Distibution.FavorBottom;
						else if(v == "top") distibution = Distibution.FavorTop;
					}
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseFloat("scale", ref scale);
					elem.TryParseFloat("variation", ref variationScale);
				}
			}

			public override void ProcessBlockColumn(Dimension dim, BlockCoord topPos, float mask, Random random)
			{
				if(Probability(amount * 0.12f * invChunkArea * (topPos.y * 0.016f) * mask))
				{
					var r = random.NextDouble();
					if(distibution == Distibution.FavorBottom)
					{
						r *= r;
					}
					else if(distibution == Distibution.FavorTop)
					{
						r = Math.Sqrt(r);
					}
					int y = (int)WorldForge.MathUtils.Lerp(yMin, yMax, (float)r);
					if(y > topPos.y) return;
					GenerateCave(dim, new SVector3(topPos.x, y, topPos.z), 0);
				}
			}


			private void GenerateCave(Dimension dim, SVector3 pos, int iteration, float maxDelta = 1f)
			{
				float delta = RandomRange(maxDelta * 0.25f, maxDelta);
				int life = (int)(RandomRange(50, 300) * delta);
				life = Math.Min(life, 400);
				float size = RandomRange(2, 7.5f * delta);
				float variation = RandomRange(0.2f, 1f) * variationScale;
				SVector3 direction = SVector3.Normalize(GetRandomVector(iteration == 0));
				float branchingChance = 0;
				bool breakSurface = Probability(0.4f);
				if(delta > 0.25f && iteration < 3)
				{
					branchingChance = size * 0.01f;
				}
				while(life > 0)
				{
					life--;
					if(!CarveSphere(dim, pos, size, breakSurface))
					{
						//Nothing was carved, the cave is dead
						return;
					}
					SVector3 newDirection = ApplyYWeights((float)pos.Y, GetRandomVector(true));
					direction += newDirection * variation * 0.5f;
					direction = SVector3.Normalize(direction);
					size = WorldForge.MathUtils.Lerp(size, RandomRange(2, 6) * delta, 0.15f);
					variation = WorldForge.MathUtils.Lerp(variation, RandomRange(0.2f, 1f) * variationScale, 0.1f);
					if(Probability(branchingChance))
					{
						//Start a new branch at the current position
						GenerateCave(dim, pos, iteration + 1, maxDelta * 0.8f);
					}
					pos += direction;
				}
			}

			private SVector3 GetRandomVector(bool allowUpwards)
			{
				return SVector3.Normalize(new SVector3()
				{
					X = RandomRange(-1, 1),
					Y = RandomRange(-1, allowUpwards ? 1 : 0),
					Z = RandomRange(-1, 1)
				});
			}

			private float Smoothstep(float v, float a, float b)
			{
				if(v <= a) return a;
				if(v >= b) return b;

				float t = Math.Min(Math.Max((v - a) / (b - a), 0), 1);
				return t * t * (3f - 2f * t);
			}

			private SVector3 ApplyYWeights(float y, SVector3 dir)
			{
				float weight = 0;
				if(y < 16)
				{
					weight = Smoothstep(1f - y / 16f, 0, 1);
				}
				return SVector3.Lerp(dir, new SVector3(0, 1, 0), weight);
			}
		}

		public class CavernCarver : Carver
		{
			private NoiseParameters noiseParameters;

			public int yMin = 4;
			public int yMax = 32;
			public int center = -999;
			public float threshold = 0.8f;
			public float scaleXZ = 1f;
			public float scaleY = 1f;
			public float noiseScale = 1f;

			public CavernCarver(XElement elem) : base(elem)
			{
				if(elem != null)
				{
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseInt("center", ref center);
					elem.TryParseFloat("threshold", ref threshold);
					elem.TryParseFloat("scale-xz", ref scaleXZ);
					elem.TryParseFloat("scale-y", ref scaleY);
					elem.TryParseFloat("noise", ref noiseScale);
				}
				if(center == -999)
				{
					center = (int)WorldForge.MathUtils.Lerp(yMin, yMax, 0.3f);
				}
				noiseParameters = new NoiseParameters(new SVector3(0.05f * scaleXZ, 0.10f * scaleY, 0.05f * scaleXZ))
				{
					fractalParameters = new FractalParameters(3, 2, 0.25f * noiseScale)
				};
			}

			public override void ProcessBlockColumn(Dimension dim, BlockCoord topPos, float mask, Random random)
			{
				for(int y = yMin; y <= Math.Min(yMax, topPos.y); y++)
				{
					float perlin = SimplexNoise.Instance.GetNoise3D(new SVector3(topPos.x, y, topPos.z), noiseParameters);
					perlin = 2f * (perlin - 0.5f) + 0.5f;

					double hw;
					if(y < center)
					{
						hw = Math.Sqrt(Math.Cos((y - center) * 3.14f / (center - yMin) * 0.5f));
					}
					else
					{
						hw = Math.Sqrt(Math.Cos((y - center) * 3.14f / (center - yMax) * 0.5f));
					}

					if(perlin * hw * mask > threshold)
					{
						CarveBlock(dim, (topPos.x, y, topPos.z), true);
					}
				}
			}
		}

		public class SpringCarver : Carver
		{

			public int yMin = 10;
			public int yMax = 80;
			public float amount = 1f;
			public bool isLavaSpring = false;

			public SpringCarver(XElement elem) : base(elem)
			{
				if(elem != null)
				{
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseFloat("amount", ref amount);
					elem.TryParseBool("lava", ref isLavaSpring);
				}
			}

			public override void ProcessBlockColumn(Dimension dim, BlockCoord topPos, float mask, Random random)
			{
				if(Probability(amount * 0.08f * mask))
				{
					int y = random.Next(yMin, yMax);
					if(y > topPos.y) return;
					TryGenerateSpring(dim, topPos, isLavaSpring ? BlockState.Lava : BlockState.Water);
				}
			}

			private void TryGenerateSpring(Dimension dim, BlockCoord pos, BlockState block)
			{
				if(CanGenerateSpring(dim, pos))
				{
					dim.SetBlock(pos, block);
					dim.MarkForTickUpdate(pos);
				}
			}

			private bool CanGenerateSpring(Dimension dim, BlockCoord pos)
			{
				if(!dim.IsDefaultBlock(pos)) return false;
				int openSides = 0;
				if(dim.IsAirNotNull(pos.West)) openSides++;
				if(dim.IsAirNotNull(pos.East)) openSides++;
				if(dim.IsAirNotNull(pos.North)) openSides++;
				if(dim.IsAirNotNull(pos.South)) openSides++;
				if(openSides >= 1 && openSides <= 2)
				{
					//Check for top and bottom
					if(!dim.IsAirOrNull(pos.Above) && !dim.IsAirOrNull(pos.Below))
					{
						return true;
					}
				}
				return false;
			}
		}

		public class CaveGenLayer : Layer
		{

			public List<Carver> carvers = new List<Carver>();

			public override void ProcessBlockColumn(Dimension dim, Random random, BlockCoord pos, float mask)
			{
				foreach(var c in carvers)
				{
					c.ProcessBlockColumn(dim, pos, mask, random);
				}
			}
		}


		public override Priority OrderPriority => Priority.AfterFirst;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;
		public override int BlockProcessYMin => 8;
		public override int BlockProcessYMax => 92;

		private Weightmap<float> weightmap;
		private Dictionary<int, Layer> caveGenLayers = new Dictionary<int, Layer>();

		public CaveGenerator(bool useDefaultGenerators)
		{
			if(useDefaultGenerators)
			{
				AddDefaultCaveGenerators();
			}
		}

		public CaveGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			weightmap = LoadWeightmap(rootPath, xml, out var weightmapXml);
			if(weightmapXml != null)
			{
				LoadLayers(weightmapXml.Elements(), CreateCaveGenLayer);
			}
			else
			{
				Console.WriteLine("Using default settings for cave gen.");
				AddDefaultCaveGenerators();
			}
		}

		private void AddDefaultCaveGenerators()
		{
			//Use default settings
			var layer = new CaveGenLayer();
			//Setting XElement to null will result in default values being used
			layer.carvers.Add(new CaveCarver(null));
			layer.carvers.Add(new CavernCarver(null));
			layer.carvers.Add(new SpringCarver(null));
			layer.carvers.Add(new SpringCarver(null) { isLavaSpring = true, amount = 0.5f });
			caveGenLayers.Add(-1, layer);
		}

		private Layer CreateCaveGenLayer(XElement elem)
		{
			CaveGenLayer layer = new CaveGenLayer();
			foreach(var carverElem in elem.Elements())
			{
				string name = carverElem.Name.LocalName.ToLower();
				if(name == "caves")
				{
					layer.carvers.Add(new CaveCarver(carverElem));
				}
				else if(name == "caverns")
				{
					layer.carvers.Add(new CavernCarver(carverElem));
				}
				else if(name == "springs")
				{
					layer.carvers.Add(new SpringCarver(carverElem));
				}
				else
				{
					throw new ArgumentException("Unknown carver type: " + name);
				}
			}
			if(layer.carvers.Count == 0)
			{
				//TODO: create logger system
				Console.WriteLine($"The layer '{elem.Name.LocalName}' is defined but has no carvers added to it.");
			}
			return layer;
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			ProcessWeightmapLayersSurface(caveGenLayers, weightmap, dimension, pos, pass, mask);
		}
	}
}

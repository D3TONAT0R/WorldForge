using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Builders;
using WorldForge.Builders.PostProcessors;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public class WorldPostProcessingStack
	{
		public PostProcessContext context;

		public List<PostProcessor> stack = new List<PostProcessor>();

		public void CreateFromXML(string importedFilePath, string xmlFilePath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var xmlString = File.ReadAllText(xmlFilePath);
			Create(Path.GetDirectoryName(importedFilePath), xmlString, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
		}

		public void CreateDefaultPostProcessor(string importedFilePath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var xmlString = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "postprocess_default.xml"));
			Create(Path.GetDirectoryName(importedFilePath), xmlString, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
		}

		public WorldPostProcessingStack(PostProcessContext context)
		{
			this.context = context;
		}

		private void Create(string rootPath, string xmlString, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var xmlRootElement = XDocument.Parse(xmlString).Root;
			LoadSettings(rootPath, xmlRootElement, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
		}

		void LoadSettings(string rootFolder, XElement xmlRootElement, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			foreach(var schematicsContainer in xmlRootElement.Descendants("schematics"))
			{
				foreach(var elem in schematicsContainer.Elements())
				{
					RegisterStructure(Path.Combine(rootFolder, elem.Value), elem.Name.LocalName);
				}
			}

			foreach(var splatXml in xmlRootElement.Element("postprocess").Elements())
			{
				LoadGenerator(splatXml, false, rootFolder, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
			}
		}

		void LoadGenerator(XElement splatXml, bool fromInclude, string rootPath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var name = splatXml.Name.LocalName.ToLower();
			if(name == "splat")
			{
				stack.Add(new WeightmappedTerrainPostProcessor(splatXml, rootPath, ditherLimit, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "water")
			{
				stack.Add(new WaterLevelPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "ores")
			{
				stack.Add(new OreGenPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "snow")
			{
				stack.Add(new SnowPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "deice")
			{
				stack.Add(new ThawingPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "naturalize")
			{
				stack.Add(new NaturalTerrainPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "vegetation")
			{
				stack.Add(new VegetationPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "caves")
			{
				stack.Add(new CavesPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "bedrock")
			{
				stack.Add(new BedrockPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if(name == "include")
			{
				if(fromInclude)
				{
					throw new InvalidOperationException("Recursive includes are not allowed");
				}
				//Include external xml
				var includePathElem = splatXml.Attribute("file");
				if(includePathElem == null)
				{
					throw new KeyNotFoundException("The include's file must be specified with a 'file' attribute");
				}
				var includePath = Path.Combine(rootPath, includePathElem.Value);

				var include = XDocument.Parse(File.ReadAllText(includePath)).Root;

				foreach(var elem in include.Elements())
				{
					if(elem.Name == "schematics")
					{
						foreach(var se in elem.Elements())
						{
							RegisterStructure(Path.Combine(rootPath, se.Value), se.Name.LocalName);
						}
					}
					else
					{
						LoadGenerator(elem, true, Path.GetDirectoryName(includePath), ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
					}
				}
			}
		}

		public bool ContinsGeneratorOfType(Type type)
		{
			foreach(var g in stack)
			{
				if(g.GetType() == type)
				{
					return true;
				}
			}
			return false;
		}

		private void RegisterStructure(string filename, string key)
		{
			try
			{
				//TODO
				//context.Schematics.Add(key, new Schematic(filename));
			}
			catch
			{
				throw new Exception("Failed to import structure '" + filename + "'");
			}
		}

		public void Process(int xMin, int zMin, int xMax, int zMax)
		{
			int processorIndex = 0;
			foreach(var post in stack)
			{
				post.Begin(context);
			}
			foreach(var post in stack)
			{
				for(int pass = 0; pass < post.PassCount; pass++)
				{
					if(post.PostProcessorType == PostProcessType.Block || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every block
						for(int x = xMin; x <= xMax; x++)
						{
							for(int z = zMin; z <= zMax; z++)
							{
								for(int y = post.BlockProcessYMin; y <= post.BlockProcessYMax; y++)
								{
									post.ProcessBlock(new BlockCoord(x, y, z), pass);
								}
							}
							//UpdateProgressBar(processorIndex, "Decorating terrain", name, (x + 1) / (float)heightmapLengthX, pass, post.NumberOfPasses);
						}
					}

					if(post.PostProcessorType == PostProcessType.Surface || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every surface block
						for(int x = xMin; x <= xMax; x++)
						{
							for(int z = zMin; z <= zMax; z++)
							{
								//TODO: remember height so each processor uses the same height
								post.ProcessSurface(new BlockCoord(x, context.Dimension.GetHighestBlock(x, z, HeightmapType.SolidBlocksNoLiquid), z), pass);
							}
							//UpdateProgressBar(processorIndex, "Decorating surface", name, (x + 1) / (float)heightmapLengthX, pass, post.NumberOfPasses);
						}
					}

					//Run every postprocessor once for every region
					Parallel.ForEach(context.Dimension.regions.Values, reg =>
					{
						post.ProcessRegion(reg, pass);
					});
				}
				processorIndex++;
			}
			foreach(var post in stack)
			{
				post.Finish();
			}
		}

		public void OnCreateWorldFiles(string worldFolder)
		{
			foreach(var post in stack)
			{
				post.OnCreateWorldFiles(worldFolder);
			}
		}
	}
}
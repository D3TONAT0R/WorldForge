using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using WorldForge.Structures;

namespace WorldForge.Builders.PostProcessors
{
    public class PostProcessingChain
	{
		public PostProcessContext Context { get; private set; }

		public List<PostProcessor> processors = new List<PostProcessor>();

		public SchematicsDatabase schematics = new SchematicsDatabase();

		public void CreateFromXML(string xmlFilePath, string fileSourceDirectory)
		{
			var xmlString = File.ReadAllText(xmlFilePath);
			Create(fileSourceDirectory, xmlString);
		}

		public void CreateFromXML(string xmlFilePath)
		{
			CreateFromXML(xmlFilePath, Path.GetDirectoryName(xmlFilePath));
		}

		public void CreateDefaultOverworldChain()
		{
			AddProcessor(new BedrockGenerator());
			AddProcessor(new NaturalSurfaceGenerator(62));
			AddProcessor(new OreGenerator(true));
			AddProcessor(new CaveGenerator(true));
			AddProcessor(new VegetationGenerator());
			AddProcessor(new SnowPostProcessor());
		}

		public PostProcessingChain()
		{

		}

		public void AddProcessor(PostProcessor processor)
		{
			processors.Add(processor);
		}

		private void Create(string fileSourceDir, string xmlString)
		{
			var xmlRootElement = XDocument.Parse(xmlString).Root;
			LoadSettings(fileSourceDir, xmlRootElement);
		}

		void LoadSettings(string rootFolder, XElement xmlRootElement)
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
				LoadGenerator(splatXml, false, rootFolder);
			}
		}

		void LoadGenerator(XElement splatXml, bool fromInclude, string rootPath)
		{
			var name = splatXml.Name.LocalName.ToLower();
			if(name == "map")
			{
				processors.Add(new WeightmappedTerrainGenerator(splatXml, rootPath));
			}
			else if(name == "water")
			{
				processors.Add(new WaterLevelGenerator(rootPath, splatXml));
			}
			else if(name == "ores")
			{
				processors.Add(new OreGenerator(rootPath, splatXml));
			}
			else if(name == "snow")
			{
				processors.Add(new SnowPostProcessor(rootPath, splatXml));
			}
			else if(name == "thaw")
			{
				processors.Add(new ThawingPostProcessor(rootPath, splatXml));
			}
			else if(name == "naturalize")
			{
				processors.Add(new NaturalSurfaceGenerator(rootPath, splatXml));
			}
			else if(name == "vegetation")
			{
				processors.Add(new VegetationGenerator(rootPath, splatXml));
			}
			else if(name == "caves")
			{
				processors.Add(new CaveGenerator(rootPath, splatXml));
			}
			else if(name == "bedrock")
			{
				processors.Add(new BedrockGenerator(rootPath, splatXml));
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
						LoadGenerator(elem, true, Path.GetDirectoryName(includePath));
					}
				}
			}
		}

		public bool ContainsGeneratorOfType(Type type)
		{
			foreach(var g in processors)
			{
				if(g.GetType() == type)
				{
					return true;
				}
			}
			return false;
		}

		private void RegisterStructure(string xmlFilePath, string key)
		{
			try
			{
				schematics.Add(key, Schematic.FromXML(xmlFilePath));
			}
			catch
			{
				throw new Exception("Failed to import structure '" + xmlFilePath + "'");
			}
		}

		public void Process(PostProcessContext context)
		{
			foreach(var post in processors)
			{
				post.Process(context);
			}
		}

		public void OnCreateWorldFiles(string worldFolder)
		{
			foreach(var post in processors)
			{
				post.OnCreateWorldFiles(worldFolder);
			}
		}
	}
}
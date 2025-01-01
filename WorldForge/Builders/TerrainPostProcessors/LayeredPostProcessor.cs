using System.Collections.Generic;
using System.Xml.Linq;
using System;

namespace WorldForge.Builders.PostProcessors
{
	public abstract class LayeredPostProcessor : PostProcessor
	{
		protected readonly Dictionary<int, Layer> layers = new Dictionary<int, Layer>();

		protected LayeredPostProcessor()
		{

		}

		protected LayeredPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ)
			: base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{

		}

		protected virtual void LoadLayers(IEnumerable<XElement> layerElements, Func<XElement, Layer> createLayerAction)
		{
			layers.Clear();
			foreach(var elem in layerElements)
			{
				string name = elem.Name.LocalName.ToLower();
				if(name == "r" || name == "red")
				{
					RegisterLayer(0, layers, createLayerAction, elem);
				}
				else if(name == "g" || name == "green")
				{
					RegisterLayer(1, layers, createLayerAction, elem);
				}
				else if(name == "b" || name == "blue")
				{
					RegisterLayer(2, layers, createLayerAction, elem);
				}
				else if(name == "a" || name == "alpha")
				{
					RegisterLayer(3, layers, createLayerAction, elem);
				}
				else if(name == "n" || name == "none")
				{
					RegisterLayer(-1, layers, createLayerAction, elem);
				}
				else
				{
					throw new ArgumentException("Unknown channel name: " + name);
				}
			}
		}

		private void RegisterLayer(int maskChannelIndex, Dictionary<int, Layer> layers, Func<XElement, Layer> createLayerAction, XElement elem)
		{
			layers.Add(maskChannelIndex, createLayerAction(elem));
		}
	}
}
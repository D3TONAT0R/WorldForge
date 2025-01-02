using System;
using System.Collections.Generic;
using WorldForge.Structures;

namespace WorldForge.Builders
{
	public class SchematicsDatabase
	{
		public readonly Dictionary<string, Schematic> data = new Dictionary<string, Schematic>();

		public bool TryGet(string name, out Schematic schematic)
		{
			return data.TryGetValue(name, out schematic);
		}

		public void Add(string name, Schematic schematic)
		{
			if(schematic == null) throw new ArgumentException("Schematic cannot be null");
			data.Add(name, schematic);
		}

		public void Remove(string name)
		{
			data.Remove(name);
		}
	}
}

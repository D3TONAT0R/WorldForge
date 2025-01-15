using System;
using System.IO;

namespace WorldForge.IO
{
	public class RegionFileStreams : IDisposable
	{
		public FileStream main;
		public FileStream entities;
		public FileStream poi;

		public RegionFileStreams(FileStream main, FileStream entities, FileStream poi)
		{
			this.main = main;
			this.entities = entities;
			this.poi = poi;
		}

		public RegionFileStreams(string mainPath, string entitiesPath, string poiPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(mainPath));
			main = new FileStream(mainPath, FileMode.Create);
			if(!string.IsNullOrEmpty(entitiesPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entitiesPath));
				entities = new FileStream(entitiesPath, FileMode.Create);
			}
			if(!string.IsNullOrEmpty(poiPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(poiPath));
				poi = new FileStream(poiPath, FileMode.Create);
			}
		}

		public RegionFileStreams(string dimensionRootPath, string fileName, bool createEntities, bool createPoi)
		{
			main = new FileStream(Path.Combine(dimensionRootPath, fileName), FileMode.Create);
			if(createEntities)
			{
				Directory.CreateDirectory(Path.Combine(dimensionRootPath, "entities"));
				entities = new FileStream(Path.Combine(dimensionRootPath, "entities", fileName), FileMode.Create);
			}
			if(createPoi)
			{
				Directory.CreateDirectory(Path.Combine(dimensionRootPath, "poi"));
				poi = new FileStream(Path.Combine(dimensionRootPath, "poi", fileName), FileMode.Create);
			}
		}

		public void Dispose()
		{
			main?.Dispose();
			entities?.Dispose();
			poi?.Dispose();
		}
	}
}

using System.IO;

namespace WorldForge.IO
{
	public class RegionFilePaths
	{
		public string mainPath;
		public string entitiesPath;
		public string poiPath;

		public bool EntitiesFileExists => !string.IsNullOrEmpty(entitiesPath) && File.Exists(entitiesPath);
		public bool POIFileExists => !string.IsNullOrEmpty(poiPath) && File.Exists(poiPath);

		public RegionFilePaths(string mainPath, string entitiesPath, string poiPath)
		{
			if(string.IsNullOrEmpty(mainPath) || !File.Exists(mainPath))
			{
				throw new System.ArgumentException("Main region file cannot be null and must exist.", nameof(mainPath));
			}
			this.mainPath = mainPath;
			this.entitiesPath = entitiesPath;
			this.poiPath = poiPath;
		}

		public RegionFileStreams OpenStreams(FileMode mode)
		{
			var main = new FileStream(mainPath, mode);
			var entities = EntitiesFileExists ? new FileStream(entitiesPath, mode) : null;
			var poi = POIFileExists ? new FileStream(poiPath, mode) : null;
			return new RegionFileStreams(main, entities, poi);
		}
	}
}
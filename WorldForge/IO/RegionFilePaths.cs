using System;
using System.IO;
using System.Threading;

namespace WorldForge.IO
{
	public class RegionFilePaths
	{
		public string mainPath;
		public string entitiesPath;
		public string poiPath;

		public bool MainFileExists => !string.IsNullOrEmpty(mainPath) && File.Exists(mainPath);
		public bool EntitiesFileExists => !string.IsNullOrEmpty(entitiesPath) && File.Exists(entitiesPath);
		public bool POIFileExists => !string.IsNullOrEmpty(poiPath) && File.Exists(poiPath);

		public DateTime MainFileLastWriteTimeUtc => MainFileExists ? File.GetLastWriteTimeUtc(mainPath) : DateTime.MinValue;
		public DateTime EntitiesFileLastWriteTimeUtc => EntitiesFileExists ? File.GetLastWriteTimeUtc(entitiesPath) : DateTime.MinValue;
		public DateTime POIFileLastWriteTimeUtc => POIFileExists ? File.GetLastWriteTimeUtc(poiPath) : DateTime.MinValue;

		public DateTime LatestTimestamp
		{
			get
			{
				DateTime timestamp = DateTime.MinValue;
				if (MainFileExists) timestamp = File.GetLastWriteTimeUtc(mainPath);
				if (EntitiesFileExists)
				{
					var entitiesTimestamp = File.GetLastWriteTimeUtc(entitiesPath);
					if (entitiesTimestamp > timestamp) timestamp = entitiesTimestamp;
				}
				if (POIFileExists)
				{
					var poiTimestamp = File.GetLastWriteTimeUtc(poiPath);
					if (poiTimestamp > timestamp) timestamp = poiTimestamp;
				}
				return timestamp;
			}
		}

		public RegionFilePaths(string mainPath, string entitiesPath, string poiPath)
		{
			if(string.IsNullOrEmpty(mainPath)) throw new ArgumentException("Main region file cannot be null.", nameof(mainPath));
			if (!File.Exists(mainPath)) throw new FileNotFoundException("Main region file does not exist: " + mainPath);
			this.mainPath = mainPath;
			this.entitiesPath = entitiesPath;
			this.poiPath = poiPath;
		}

		public RegionFileStreams OpenStreams(SemaphoreSlim semaphore)
		{
			// Use semaphore to avoid reading multiple files at the same time, causing a performance penalty.
			semaphore?.Wait();
			var main = new MemoryStream(File.ReadAllBytes(mainPath));
			var entities = EntitiesFileExists ? new MemoryStream(File.ReadAllBytes(entitiesPath)) : null;
			var poi = POIFileExists ? new MemoryStream(File.ReadAllBytes(poiPath)) : null;
			semaphore?.Release();
			return new RegionFileStreams(main, entities, poi);
		}
	}
}
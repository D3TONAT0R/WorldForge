using System.IO;

namespace WorldForge.IO
{
	public class RegionFileStreams
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
	}
}

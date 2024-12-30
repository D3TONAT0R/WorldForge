namespace WorldForge.Builders.PostProcessors
{
	public class PostProcessContext
	{
		public Dimension Dimension { get; private set; }
		public SchematicsDatabase Schematics { get; private set; }

		public GameVersion TargetVersion { get; set; }

		public PostProcessContext(Dimension dimension, SchematicsDatabase schematics, GameVersion? targetVersion = null)
		{
			Dimension = dimension;
			Schematics = schematics;
			TargetVersion = targetVersion ?? GameVersion.DefaultVersion;
		}

		public PostProcessContext(Dimension dimension, GameVersion? targetVersion = null) : this(dimension, new SchematicsDatabase())
		{

		}
	}
}
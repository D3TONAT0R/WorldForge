namespace WorldForge.Builders.PostProcessors
{
    public class PostProcessContext
	{
		public Dimension Dimension { get; private set; }
		public SchematicsDatabase Schematics { get; private set; }
		public Boundary Boundary { get; private set; }

		public GameVersion TargetVersion { get; set; }

		public PostProcessContext(Dimension dimension, Boundary boundary, SchematicsDatabase schematics, GameVersion? targetVersion = null)
		{
			Dimension = dimension;
			Boundary = boundary;
			Schematics = schematics;
			TargetVersion = targetVersion ?? GameVersion.DefaultVersion;
		}

		public PostProcessContext(Dimension dimension, Boundary boundary, GameVersion? targetVersion = null) : this(dimension, boundary, new SchematicsDatabase(), targetVersion)
		{

		}
	}
}
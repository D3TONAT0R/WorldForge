namespace WorldForge.Builders.PostProcessors
{
    public class PostProcessContext
	{
		public Dimension Dimension { get; private set; }
		public SchematicsDatabase Schematics { get; private set; }
		public Boundary Boundary { get; private set; }

		//Uses world seed by default
		public long BaseSeed { get; set; }

		public PostProcessor CurrentPostProcessor { get; set; }
		public int CurrentPassIndex { get; set; }

		public GameVersion TargetVersion { get; set; }

		public PostProcessContext(Dimension dimension, Boundary boundary, SchematicsDatabase schematics, GameVersion? targetVersion = null, long? baseSeed = null)
		{
			Dimension = dimension;
			Boundary = boundary;
			Schematics = schematics;
			TargetVersion = targetVersion ?? GameVersion.DefaultVersion;
			BaseSeed = baseSeed ?? dimension.ParentWorld?.LevelData.worldGen.WorldSeed ?? 0;
		}

		public PostProcessContext(Dimension dimension, Boundary boundary, GameVersion? targetVersion = null, long? baseSeed = null) : this(dimension, boundary, new SchematicsDatabase(), targetVersion, baseSeed)
		{

		}

		public void SetCurrentState(PostProcessor current, int pass)
		{
			CurrentPostProcessor = current;
			CurrentPassIndex = pass;
		}

		public void ClearCurrentState()
		{
			CurrentPostProcessor = null;
			CurrentPassIndex = 0;
		}
	}
}
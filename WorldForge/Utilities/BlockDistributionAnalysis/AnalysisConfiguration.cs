namespace WorldForge.Utilities.BlockDistributionAnalysis
{
    public class AnalysisConfiguration
    {
        public string Name { get; private set; }

        public BlockGroup[] BlockGroups { get; private set; }

        public AnalysisConfiguration(string name, params BlockGroup[] groups)
        {
            Name = name;
            BlockGroups = groups;
        }

        public override string ToString() => Name;
    }
}
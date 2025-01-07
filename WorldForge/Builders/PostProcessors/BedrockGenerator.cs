using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class BedrockGenerator : PostProcessor
	{
		public bool Noisy { get; set; } = true;

		public int Level { get; set; } = 0;

		public bool ForcePlacement { get; set; } = false;

		public override Priority OrderPriority => Priority.First;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public BedrockGenerator()
		{

		}

		public BedrockGenerator(string rootPath, XElement xml)
			: base(rootPath, xml)
		{

		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			pos.y = Level;
			TrySetBedrock(dim, pos);
			if(Noisy)
			{
				var seed = Seed;
				for(int i = 1; i < 4; i++)
				{
					pos.y = Level + i;
					if(SeededRandom.Probability(1f - i / 4f, seed, pos)) TrySetBedrock(dim, pos);
				}
			}
		}

		private void TrySetBedrock(Dimension dim, BlockCoord pos)
		{
			if(dim.IsAirOrNull(pos) && !ForcePlacement) return;
			dim.SetBlock(pos, Blocks.bedrock);
		}
	}
}
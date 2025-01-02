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

		public BedrockGenerator(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ)
			: base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{

		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			pos.y = Level;
			TrySetBedrock(dim, pos);
			if(Noisy)
			{
				for(int i = 1; i < 4; i++)
				{
					pos.y = Level + i;
					if(Probability(1f - i / 4f)) TrySetBedrock(dim, pos.ShiftVertical(i));
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
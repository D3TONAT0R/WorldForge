using WorldForge;

namespace NetheriteFinder
{
	public class SearchProfile
	{
		public static readonly SearchProfile Netherite = new SearchProfile(
			"Netherite",
			["ancient_debris"],
			"minecraft:the_nether", 
			1, 32,
			12, 18,
			Brushes.DarkOrange
		);

		public static readonly SearchProfile Diamond = new SearchProfile(
			"Diamond",
			["deepslate_diamond_ore", "diamond_ore"],
			"minecraft:overworld",
			-63, 20,
			-63, -56,
			Brushes.DarkTurquoise
		);

		public readonly string name;
		public readonly BlockID[] blocks;
		public readonly string dimensionId;
		public readonly int searchYMin;
		public readonly int searchYMax;
		public readonly int displayYMin;
		public readonly int displayYMax;
		public readonly Brush veinBrush;

		public SearchProfile(string name, string[] blockNames, string dimensionId, int searchYMin, int searchYMax, int displayYMin, int displayYMax, Brush veinBrush)
		{
			this.name = name;
			blocks = blockNames.Select(n => BlockList.Find(n, true)).ToArray();
			this.dimensionId = dimensionId;
			this.searchYMin = searchYMin;
			this.searchYMax = searchYMax;
			this.displayYMin = displayYMin;
			this.displayYMax = displayYMax;
			this.veinBrush = veinBrush;
		}

		public override string ToString()
		{
			return name;
		}
	}
}
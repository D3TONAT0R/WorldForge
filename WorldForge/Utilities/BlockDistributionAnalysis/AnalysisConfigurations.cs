namespace WorldForge.Utilities.BlockDistributionAnalysis
{
	public static class AnalysisConfigurations
	{
		public static readonly AnalysisConfiguration OverworldOres = new AnalysisConfiguration(
			"Overworld Ores",
			BlockGroup.Parse("Coal;coal_ore,deepslate_coal_ore"),
			BlockGroup.Parse("Iron;iron_ore,deepslate_iron_ore"),
			BlockGroup.Parse("Iron Block;raw_iron_block"),
			BlockGroup.Parse("Gold;gold_ore,deepslate_gold_ore"),
			BlockGroup.Parse("Gold Block;raw_gold_block"),
			BlockGroup.Parse("Copper;copper_ore,deepslate_copper_ore"),
			BlockGroup.Parse("Copper Block;raw_copper_block"),
			BlockGroup.Parse("Diamond;diamond_ore,deepslate_diamond_ore"),
			BlockGroup.Parse("Emerald;emerald_ore,deepslate_emerald_ore"),
			BlockGroup.Parse("Lapis Lazuli;lapis_ore,deepslate_lapis_ore"),
			BlockGroup.Parse("Redstone;redstone_ore,deepslate_redstone_ore"),
			BlockGroup.Parse("Amethyst;amethyst_block,budding_amethyst")
		);

		public static readonly AnalysisConfiguration NetherOres = new AnalysisConfiguration(
			"Nether Ores",
			BlockGroup.Parse("Nether Quartz;nether_quartz_ore"),
			BlockGroup.Parse("Nether Gold;nether_gold_ore"),
			BlockGroup.Parse("Ancient Debris;ancient_debris")
		);

		public static readonly AnalysisConfiguration Air = new AnalysisConfiguration(
			"Air",
			BlockGroup.Parse("Air;air,cave_air")
		);

        public static readonly AnalysisConfiguration Liquids = new AnalysisConfiguration(
            "Liquids",
            BlockGroup.Parse("Water;water"),
            BlockGroup.Parse("Lava;lava")
        );

        public static readonly AnalysisConfiguration Stones = new AnalysisConfiguration(
			"Stones",
			BlockGroup.Parse("Stone;stone"),
			BlockGroup.Parse("Deepslate;deepslate"),
			BlockGroup.Parse("Sandstone;sandstone,red_sandstone"),
			BlockGroup.Parse("Andesite;andesite"),
			BlockGroup.Parse("Diorite;diorite"),
			BlockGroup.Parse("Granite;granite"),
			BlockGroup.Parse("Tuff;tuff"),
			BlockGroup.Parse("Calcite;calcite"),
			BlockGroup.Parse("Basalt;basalt,smooth_basalt")
		);

		public static readonly AnalysisConfiguration TerrainBlocks = new AnalysisConfiguration(
			"Terrain Blocks",
			BlockGroup.Parse("Dirt;dirt,grass_block,podzol,rooted_dirt,coarse_dirt"),
			BlockGroup.Parse("Sand;sand,red_sand"),
			BlockGroup.Parse("Gravel;gravel"),
			BlockGroup.Parse("Clay;clay"),
			BlockGroup.Parse("Snow;snow,snow_block"),
			BlockGroup.Parse("Ice;ice,packed_ice,blue_ice")
		);
	}
}
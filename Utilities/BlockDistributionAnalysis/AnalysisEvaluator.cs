using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MCUtils.Utilities.BlockDistributionAnalysis
{
	public static class AnalysisEvaluator
	{
		[Flags]
		public enum TargetBlockTypes
		{
			None = 0,
			Ores = 1,
			AirAndLiquids = 2,
			Stones = 4,
			TerrainBlocks = 8
		}

		static readonly BlockGroup[] ores = new BlockGroup[]
		{
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
			BlockGroup.Parse("Nether Quartz;nether_quartz_ore"),
			BlockGroup.Parse("Nether Gold;nether_gold_ore"),
			BlockGroup.Parse("Ancient Debris;ancient_debris"),
			BlockGroup.Parse("Amethyst;amethyst_block,budding_amethyst")
		};

		static readonly BlockGroup[] airAndLiquids = new BlockGroup[]
		{
			BlockGroup.Parse("Air;air,cave_air"),
			BlockGroup.Parse("Water;water"),
			BlockGroup.Parse("Lava;lava")
		};

		static readonly BlockGroup[] stones = new BlockGroup[]
		{
			BlockGroup.Parse("Stone;stone"),
			BlockGroup.Parse("Deepslate;deepslate"),
			BlockGroup.Parse("Sandstone;sandstone"),
			BlockGroup.Parse("Andesite;andesite"),
			BlockGroup.Parse("Diorite;diorite"),
			BlockGroup.Parse("Granite;granite"),
			BlockGroup.Parse("Tuff;tuff"),
			BlockGroup.Parse("Calcite;calcite"),
			BlockGroup.Parse("Basalt;basalt,smooth_basalt")
		};

		static readonly BlockGroup[] terrainBlocks = new BlockGroup[]
		{
			BlockGroup.Parse("Dirt;dirt,grass_block,podzol,rooted_dirt,coarse_dirt"),
			BlockGroup.Parse("Gravel;gravel"),
			BlockGroup.Parse("Sand;sand,red_sand"),
			BlockGroup.Parse("Clay;clay"),
			BlockGroup.Parse("Mud;mud")
		};

		public class BlockGroup
		{
			public string name;
			public ProtoBlock[] blocks;

			public BlockGroup(string name, params ProtoBlock[] blocks)
			{
				this.name = name;
				this.blocks = blocks;
			}

			public static BlockGroup Parse(string s)
			{
				var split = s.Split(';');
				var blocks = new List<ProtoBlock>(split[1].Split(',').Select(s1 => BlockList.Find(s1)));
				blocks.RemoveAll(b => b == null);
				return new BlockGroup(split[0], blocks.ToArray());
			}
		}

		public static List<BlockGroup> GetTargetBlocks(TargetBlockTypes typeFlags)
		{
			List<BlockGroup> groups = new List<BlockGroup>();
			if(typeFlags.HasFlag(TargetBlockTypes.Ores)) groups.AddRange(ores);
			if(typeFlags.HasFlag(TargetBlockTypes.AirAndLiquids)) groups.AddRange(airAndLiquids);
			if(typeFlags.HasFlag(TargetBlockTypes.Stones)) groups.AddRange(stones);
			if(typeFlags.HasFlag(TargetBlockTypes.TerrainBlocks)) groups.AddRange(terrainBlocks);
			return groups;
		}

		public static Dictionary<short, double> Evaluate(AnalysisData analysis, params string[] blockIDs)
		{
			Dictionary<short, long> combined = new Dictionary<short, long>();
			foreach(var block in blockIDs)
			{
				if(analysis.data.TryGetValue(block, out var c))
				{
					JoinDictionaries(c.counts, combined);
				}
			}
			Dictionary<short, double> results = new Dictionary<short, double>();
			foreach(var kv in combined)
			{
				results.Add(kv.Key, kv.Value / (double)analysis.chunkCounter / 256d);
			}
			return results;
		}

		public static Dictionary<short, double> Evaluate(AnalysisData analysis, BlockGroup group)
		{
			return Evaluate(analysis, group.blocks.Select(b => b.ID).ToArray());
		}

		private static void JoinDictionaries(Dictionary<short, long> src, Dictionary<short, long> target)
		{
			foreach(var kv in src)
			{
				if(!target.ContainsKey(kv.Key))
				{
					target.Add(kv.Key, 0);
				}
				target[kv.Key] += kv.Value;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldForge.Utilities.BlockDistributionAnalysis
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
			public BlockID[] blocks;

			public BlockGroup(string name, params BlockID[] blocks)
			{
				this.name = name;
				this.blocks = blocks;
			}

			public static BlockGroup Parse(string s)
			{
				var split = s.Split(';');
				var blocks = new List<BlockID>(split[1].Split(',').Select(s1 => BlockList.Find(s1)));
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

		public static Dictionary<short, double?> Evaluate(AnalysisData analysis, bool relativeToStone, params string[] blockIDs)
		{
			Dictionary<short, long> combined = new Dictionary<short, long>();
			foreach(var block in blockIDs)
			{
				if(analysis.data.TryGetValue(block, out var c))
				{
					JoinDictionaries(c.counts, combined);
				}
			}
			Dictionary<short, double?> results = new Dictionary<short, double?>();
			foreach(var kv in combined)
			{
				short y = kv.Key;
				double? rate;
				if(relativeToStone)
				{
					long stoneCount = CountTerrainBlocksAtY(analysis, y);
					if(stoneCount < analysis.chunkCounter * 0.35)
					{
						//Not enough stone at this height, results are not reliable enough
						rate = null;
					}
					else
					{
						rate = kv.Value / (double)(kv.Value + stoneCount);
					}
				}
				else
				{
					rate = kv.Value / (double)analysis.chunkCounter / 256d;
				}
				results.Add(y, rate);
			}
			return results;
		}

		private static long CountTerrainBlocksAtY(AnalysisData analysis, short y)
		{
			return
				analysis.GetTotalAtY("minecraft:stone", y) +
				analysis.GetTotalAtY("minecraft:deepslate", y) +
				analysis.GetTotalAtY("minecraft:andesite", y) +
				analysis.GetTotalAtY("minecraft:diorite", y) +
				analysis.GetTotalAtY("minecraft:granite", y) +
				analysis.GetTotalAtY("minecraft:tuff", y) +
				analysis.GetTotalAtY("minecraft:gravel", y) +
				analysis.GetTotalAtY("minecraft:dirt", y) +
				analysis.GetTotalAtY("minecraft:grass_block", y) +
				analysis.GetTotalAtY("minecraft:podzol", y) +
				analysis.GetTotalAtY("minecraft:clay", y) +
				analysis.GetTotalAtY("minecraft:mud", y) +
				analysis.GetTotalAtY("minecraft:sand", y);
		}

		public static Dictionary<short, double?> Evaluate(AnalysisData analysis, BlockGroup group, bool relativeToStone)
		{
			return Evaluate(analysis, relativeToStone, group.blocks.Select(b => b.ID).ToArray());
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

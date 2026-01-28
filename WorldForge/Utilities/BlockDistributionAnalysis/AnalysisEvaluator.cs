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

		public static List<BlockGroup> GetTargetBlocks(TargetBlockTypes typeFlags)
		{
			List<BlockGroup> groups = new List<BlockGroup>();
			if (typeFlags.HasFlag(TargetBlockTypes.Ores))
			{
				groups.AddRange(AnalysisConfigurations.OverworldOres.BlockGroups);
				groups.AddRange(AnalysisConfigurations.NetherOres.BlockGroups);
			}
			if (typeFlags.HasFlag(TargetBlockTypes.AirAndLiquids))
			{
				groups.AddRange(AnalysisConfigurations.Air.BlockGroups);
				groups.AddRange(AnalysisConfigurations.Liquids.BlockGroups);
			}
			if(typeFlags.HasFlag(TargetBlockTypes.Stones)) groups.AddRange(AnalysisConfigurations.Stones.BlockGroups);
			if(typeFlags.HasFlag(TargetBlockTypes.TerrainBlocks)) groups.AddRange(AnalysisConfigurations.TerrainBlocks.BlockGroups);
			return groups;
		}

		public static Dictionary<short, double?> Evaluate(AnalysisData analysis, bool relativeToStone, params NamespacedID[] blockIDs)
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
				analysis.GetTotalAtY(new NamespacedID("stone"), y) +
				analysis.GetTotalAtY(new NamespacedID("deepslate"), y) +
				analysis.GetTotalAtY(new NamespacedID("andesite"), y) +
				analysis.GetTotalAtY(new NamespacedID("diorite"), y) +
				analysis.GetTotalAtY(new NamespacedID("granite"), y) +
				analysis.GetTotalAtY(new NamespacedID("tuff"), y) +
				analysis.GetTotalAtY(new NamespacedID("gravel"), y) +
				analysis.GetTotalAtY(new NamespacedID("dirt"), y) +
				analysis.GetTotalAtY(new NamespacedID("grass_block"), y) +
				analysis.GetTotalAtY(new NamespacedID("podzol"), y) +
				analysis.GetTotalAtY(new NamespacedID("clay"), y) +
				analysis.GetTotalAtY(new NamespacedID("mud"), y) +
				analysis.GetTotalAtY(new NamespacedID("sand"), y);
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

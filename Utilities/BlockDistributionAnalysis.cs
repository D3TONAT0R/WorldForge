using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCUtils
{
	public class BlockDistributionAnalysis
	{
		[Flags]
		public enum TargetBlockTypes
		{
			None = 0,
			Ores = 1,
			AirAndLiquids = 2,
			Stones = 3,
			TerrainBlocks = 4
		}

		static readonly string[] ores = new string[]
		{
			"Coal;coal_ore,deepslate_coal_ore",
			"Iron;iron_ore,deepslate_iron_ore",
			"Iron Block;raw_iron_block",
			"Gold;gold_ore,deepslate_gold_ore",
			"Gold Block;raw_gold_block",
			"Copper;copper_ore,deepslate_copper_ore",
			"Copper Block;raw_copper_block",
			"Diamond;diamond_ore,deepslate_diamond_ore",
			"Emerald;emerald_ore,deepslate_emerald_ore",
			"Lapis Lazuli;lapis_ore,deepslate_lapis_ore",
			"Redstone;redstone_ore,deepslate_redstone_ore",
			"Nether Quartz;nether_quartz_ore",
			"Nether Gold;nether_gold_ore",
			"Ancient Debris;ancient_debris",
			"Amethyst;amethyst_block,budding_amethyst"
		};

		static readonly string[] airAndLiquids = new string[]
		{
			"Air;air,cave_air",
			"Water;water",
			"Lava;lava"
		};

		static readonly string[] stones = new string[]
		{
			"Stone;stone",
			"Deepslate;deepslate",
			"Sandstone;sandstone",
			"Andesite;andesite",
			"Diorite;diorite",
			"Granite;granite",
			"Tuff;tuff",
			"Calcite;calcite",
			"Basalt;basalt,smooth_basalt"
		};

		static readonly string[] terrainBlocks = new string[]
		{
			"Dirt;dirt,grass_block,podzol,rooted_dirt,coarse_dirt",
			"Gravel;gravel",
			"Sand;sand,red_sand",
			"Clay;clay",
			"Mud;mud"
		};

		public class BlockGroup
		{
			public string name;
			public ProtoBlock[] blocks;

			public BlockGroup(string name, ProtoBlock[] blocks)
			{
				this.name = name;
				this.blocks = blocks;
			}
		}

		public TargetBlockTypes BlockTypes { get; private set; }
		public short YMin { get; private set; }
		public short YMax { get; private set; }
		public int CountedChunks { get; private set; }

		private List<BlockGroup> blockGroups;
		public Dictionary<ProtoBlock, Dictionary<short, long>> results;

		public static List<BlockGroup> GetTargetBlocks(TargetBlockTypes typeFlags)
		{
			List<BlockGroup> groups = new List<BlockGroup>();
			if(typeFlags.HasFlag(TargetBlockTypes.Ores)) RegisterGroups(ores, groups);
			if(typeFlags.HasFlag(TargetBlockTypes.AirAndLiquids)) RegisterGroups(airAndLiquids, groups);
			if(typeFlags.HasFlag(TargetBlockTypes.Stones)) RegisterGroups(stones, groups);
			if(typeFlags.HasFlag(TargetBlockTypes.TerrainBlocks)) RegisterGroups(terrainBlocks, groups);
			return groups;
		}

		private static void RegisterGroups(string[] input, List<BlockGroup> groups)
		{
			foreach(var g in input) RegisterGroup(g, groups);
		}

		private static void RegisterGroup(string queryInput, List<BlockGroup> groups)
		{
			var split = queryInput.Split(';');
			var blocks = new List<ProtoBlock>(split[1].Split(',').Select(s => BlockList.Find(s)));
			blocks.RemoveAll(b => b == null);
			groups.Add(new BlockGroup(split[0], blocks.ToArray()));
		}

		public BlockDistributionAnalysis(List<BlockGroup> groups, short yMin = -64, short yMax = 320)
		{
			blockGroups = groups;
			var searchQuery = new List<ProtoBlock>();
			foreach(var g in groups)
			{
				foreach(var b in g.blocks)
				{
					if(!searchQuery.Contains(b)) searchQuery.Add(b);
				}
			}

			YMin = yMin;
			YMax = yMax;
			results = new Dictionary<ProtoBlock, Dictionary<short, long>>();
			foreach(var pb in searchQuery)
			{
				Dictionary<short, long> counter = new Dictionary<short, long>();
				for(short y = YMin; y < YMax; y++)
				{
					counter.Add(y, 0);
				}
				results.Add(pb, counter);
			}
		}

		public BlockDistributionAnalysis(TargetBlockTypes targetFlags, short yMin = -64, short yMax = 320) : this(GetTargetBlocks(targetFlags), yMin, yMax)
		{

		}

		public void AnalyzeChunk(ChunkData chunk)
		{
			try
			{
				if(chunk.HasFullyGeneratedTerrain)
				{
					chunk.ForEachBlock(YMin, YMax, (pos, b) =>
					{
						if(b != null && results.TryGetValue(b.block, out var stat))
						{
							stat[(short)pos.y]++;
						}
					});
					CountedChunks++;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"Failed to read chunk at {chunk.worldSpaceCoord}: " + e.Message);
			}
		}

		public void AnalyzeRegion(Region region, bool parallel = true)
		{
			if(parallel)
			{
				Parallel.For(0, 1024, (i) =>
				{
					var chunk = region.chunks[i % 32, i / 32];
					if(chunk != null)
					{
						AnalyzeChunk(chunk);
					}
				});
			}
			else
			{
				for(int i = 0; i < 1024; i++)
				{
					var chunk = region.chunks[i % 32, i / 32];
					if(chunk != null)
					{
						AnalyzeChunk(chunk);
					}
				}
			}
		}

		public string GenerateResultsAsCSV()
		{
			char sep = ',';
			StringBuilder csv = new StringBuilder();
			ProtoBlock[] queries = results.Keys.ToArray();
			csv.Append("");
			foreach(var group in blockGroups)
			{
				csv.Append(sep + group.name);
			}
			csv.Append(sep + "Chunk count: " + CountedChunks);
			csv.AppendLine();
			for(short y = YMin; y < YMax; y++)
			{
				csv.Append(y);
				foreach(var g in blockGroups)
				{
					long counted = 0;
					foreach(var b in g.blocks) counted += results[b][y];
					//value = amount of blocks per chunk
					double amount = counted / (double)CountedChunks;
					csv.Append(sep + amount.ToString());
				}
				csv.AppendLine();
			}
			return csv.ToString();
		}

		public void SaveAsCSVInWorldFolder(string worldFolder)
		{
			string csv = GenerateResultsAsCSV();
			string path = Path.Combine(worldFolder, "block_distribution.csv");
			File.WriteAllText(path, csv);
			Console.WriteLine("Results written to " + path);
		}
	}
}

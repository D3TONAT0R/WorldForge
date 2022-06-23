using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCUtils;
using MCUtils.Coordinates;

namespace MCUtils.ConsoleTools
{
	public class BlockDistributionConsoleTool : IConsoleTool
	{

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

		class BlockGroup
		{
			public string name;
			public ProtoBlock[] blocks;

			public BlockGroup(string name, ProtoBlock[] blocks)
			{
				this.name = name;
				this.blocks = blocks;
			}
		}

		public void Run(string[] args)
		{
			string path;
			if (args.Length > 0)
			{
				path = args[0];
			}
			else
			{
				Console.WriteLine("Enter path to world folder:");
				path = Console.ReadLine().Replace("\"", "");
			}

			Console.WriteLine("Select a search pattern:");
			Console.WriteLine("[1] Ores only");
			Console.WriteLine("[2] Ores + Air + Liquids");
			Console.WriteLine("[3] Ores + Air + Liquids + Stones");
			Console.WriteLine("[4] Ores + Air + Liquids + Stones + Terrain");
			int choice;
			while (!GetChoice(out choice))
			{
				Console.WriteLine("Invalid input.");
			}

			List<BlockGroup> groups = new List<BlockGroup>();
			foreach (var q in ores) RegisterGroup(q, groups);
			if (choice >= 2) foreach (var q in airAndLiquids) RegisterGroup(q, groups);
			if (choice >= 3) foreach (var q in stones) RegisterGroup(q, groups);
			if (choice >= 4) foreach (var q in terrainBlocks) RegisterGroup(q, groups);

			World.GetWorldInfo(path, out var worldName, out var version, out var regionLocations);

			Console.WriteLine($"World: {worldName}, Version {version}");

			CountSeparateYLevels(groups, path, regionLocations, -64, 320);
		}

		static void RegisterGroup(string queryInput, List<BlockGroup> groups)
		{
			var split = queryInput.Split(';');
			var blocks = new List<ProtoBlock>(split[1].Split(',').Select(s => BlockList.Find(s)));
			blocks.RemoveAll(b => b == null);
			groups.Add(new BlockGroup(split[0], blocks.ToArray()));
		}

		static void CountSeparateYLevels(List<BlockGroup> groups, string worldRoot, RegionLocation[] regions, short yMin, short yMax)
		{
			var searchQuery = new List<ProtoBlock>();
			foreach (var g in groups)
			{
				foreach (var b in g.blocks)
				{
					if (!searchQuery.Contains(b)) searchQuery.Add(b);
				}
			}
			int countedChunks = 0;
			Dictionary<ProtoBlock, Dictionary<short, long>> counts = new Dictionary<ProtoBlock, Dictionary<short, long>>();
			foreach (var pb in searchQuery)
			{
				Dictionary<short, long> counter = new Dictionary<short, long>();
				for (short y = yMin; y < yMax; y++)
				{
					counter.Add(y, 0);
				}
				counts.Add(pb, counter);
			}

			for (int ri = 0; ri < regions.Length; ri++)
			{
				Console.WriteLine($"Processing region {regions[ri].ToFileName()} [{ri + 1}/{regions.Length}]");
				try
				{
					var region = RegionLoader.LoadRegion(Path.Combine(worldRoot, "region", regions[ri].ToFileName()));
					Console.WriteLine("Region loaded, processing chunks ...");
					Parallel.For(0, 1024, i => {
						var c = region.chunks[i % 32, i / 32];
						if (c != null)
						{
							try
							{
								foreach (var kv in c.sections)
								{
									var section = kv.Value;
									short baseY = (short)(kv.Key * 16);
									for (byte y = 0; y < 16; y++)
									{
										short worldY = (short)(baseY + y);
										if (worldY < yMin || worldY >= yMax) continue;
										for (byte z = 0; z < 16; z++)
										{
											for (byte x = 0; x < 16; x++)
											{
												var b = section.GetBlockAt(x, y, z).block;
												if (b != null && counts.TryGetValue(b, out var stat))
												{
													stat[worldY]++;
												}
											}
										}
									}
								}
								countedChunks++;
							}
							catch (Exception e)
							{
								Console.WriteLine($"Chunk read failed @ r.{region.regionPos.x}.{region.regionPos.z} [{c.coords}]: {e.Message}");
							}
						}
					});
				}
				catch (Exception e)
				{
					Console.WriteLine("Region processing failed, skipping ...");
				}
			}
			Console.WriteLine("Preparing data ...");
			char sep = ',';
			StringBuilder csv = new StringBuilder();
			ProtoBlock[] queries = counts.Keys.ToArray();
			csv.Append("");
			foreach (var group in groups)
			{
				csv.Append(sep + group.name);
			}
			csv.Append(sep + "Chunk count: " + countedChunks);
			csv.AppendLine();
			for (short y = yMin; y < yMax; y++)
			{
				csv.Append(y);
				foreach (var g in groups)
				{
					long counted = 0;
					foreach (var b in g.blocks) counted += counts[b][y];
					//value = amount of blocks per chunk
					double amount = counted / (double)countedChunks;
					csv.Append(sep + amount.ToString());
				}
				csv.AppendLine();
			}
			string filename = Path.Combine(worldRoot, "ore_distribution.csv");
			File.WriteAllText(filename, csv.ToString());
			Console.WriteLine("Data written to " + filename);
		}

		static bool GetChoice(out int choice)
		{
			var k = Console.ReadKey(true).KeyChar;
			if (k == '1')
			{
				choice = 1;
				return true;
			}
			else if (k == '2')
			{
				choice = 2;
				return true;
			}
			else
			{
				choice = 0;
				return false;
			}
		}
	}
}

using System.Collections.Generic;

namespace WorldForge
{
	public static class BlockList
	{

		public class Remapping
		{
			public BlockID oldID;
			public BlockID newID;
			public GameVersion version;

			public Remapping(BlockID oldID, BlockID newID, GameVersion version)
			{
				this.oldID = oldID;
				this.newID = newID;
				this.version = version;
			}
		}

		public static string[] blocks = new string[] {
			"stone",
			"granite",
			"polished_granite",
			"diorite",
			"polished_diorite",
			"andesite",
			"polished_andesite",
			"grass_block",
			"dirt",
			"coarse_dirt",
			"podzol",
			"cobblestone",
			"oak_planks",
			"spruce_planks",
			"birch_planks",
			"jungle_planks",
			"acacia_planks",
			"dark_oak_planks",
			"oak_sapling",
			"spruce_sapling",
			"birch_sapling",
			"jungle_sapling",
			"acacia_sapling",
			"dark_oak_sapling",
			"bedrock",
			"sand",
			"red_sand",
			"gravel",
			"gold_ore",
			"iron_ore",
			"coal_ore",
			"oak_log",
			"spruce_log",
			"birch_log",
			"jungle_log",
			"acacia_log",
			"dark_oak_log",
			"stripped_oak_log",
			"stripped_spruce_log",
			"stripped_birch_log",
			"stripped_jungle_log",
			"stripped_acacia_log",
			"stripped_dark_oak_log",
			"stripped_oak_wood",
			"stripped_spruce_wood",
			"stripped_birch_wood",
			"stripped_jungle_wood",
			"stripped_acacia_wood",
			"stripped_dark_oak_wood",
			"oak_wood",
			"spruce_wood",
			"birch_wood",
			"jungle_wood",
			"acacia_wood",
			"dark_oak_wood",
			"oak_leaves",
			"spruce_leaves",
			"birch_leaves",
			"jungle_leaves",
			"acacia_leaves",
			"dark_oak_leaves",
			"sponge",
			"wet_sponge",
			"glass",
			"lapis_ore",
			"lapis_block",
			"dispenser",
			"sandstone",
			"chiseled_sandstone",
			"cut_sandstone",
			"note_block",
			"powered_rail",
			"detector_rail",
			"sticky_piston",
			"cobweb",
			"grass",
			"fern",
			"dead_bush",
			"seagrass",
			"sea_pickle",
			"piston",
			"white_wool",
			"orange_wool",
			"magenta_wool",
			"light_blue_wool",
			"yellow_wool",
			"lime_wool",
			"pink_wool",
			"gray_wool",
			"light_gray_wool",
			"cyan_wool",
			"purple_wool",
			"blue_wool",
			"brown_wool",
			"green_wool",
			"red_wool",
			"black_wool",
			"dandelion",
			"poppy",
			"blue_orchid",
			"allium",
			"azure_bluet",
			"red_tulip",
			"orange_tulip",
			"white_tulip",
			"pink_tulip",
			"oxeye_daisy",
			"brown_mushroom",
			"red_mushroom",
			"gold_block",
			"iron_block",
			"oak_slab",
			"spruce_slab",
			"birch_slab",
			"jungle_slab",
			"acacia_slab",
			"dark_oak_slab",
			"stone_slab",
			"sandstone_slab",
			"petrified_oak_slab",
			"cobblestone_slab",
			"brick_slab",
			"stone_brick_slab",
			"nether_brick_slab",
			"quartz_slab",
			"red_sandstone_slab",
			"purpur_slab",
			"prismarine_slab",
			"prismarine_brick_slab",
			"dark_prismarine_slab",
			"smooth_quartz",
			"smooth_red_sandstone",
			"smooth_sandstone",
			"smooth_stone",
			"bricks",
			"tnt",
			"bookshelf",
			"mossy_cobblestone",
			"obsidian",
			"torch",
			"end_rod",
			"chorus_plant",
			"chorus_flower",
			"purpur_block",
			"purpur_pillar",
			"purpur_stairs",
			"spawner",
			"oak_stairs",
			"chest",
			"diamond_ore",
			"diamond_block",
			"crafting_table",
			"farmland",
			"furnace",
			"ladder",
			"rail",
			"cobblestone_stairs",
			"lever",
			"stone_pressure_plate",
			"oak_pressure_plate",
			"spruce_pressure_plate",
			"birch_pressure_plate",
			"jungle_pressure_plate",
			"acacia_pressure_plate",
			"dark_oak_pressure_plate",
			"redstone_ore",
			"redstone_torch",
			"stone_button",
			"snow",
			"ice",
			"snow_block",
			"cactus",
			"clay",
			"jukebox",
			"oak_fence",
			"spruce_fence",
			"birch_fence",
			"jungle_fence",
			"acacia_fence",
			"dark_oak_fence",
			"pumpkin",
			"carved_pumpkin",
			"netherrack",
			"soul_sand",
			"glowstone",
			"jack_o_lantern",
			"oak_trapdoor",
			"spruce_trapdoor",
			"birch_trapdoor",
			"jungle_trapdoor",
			"acacia_trapdoor",
			"dark_oak_trapdoor",
			"infested_stone",
			"infested_cobblestone",
			"infested_stone_bricks",
			"infested_mossy_stone_bricks",
			"infested_cracked_stone_bricks",
			"infested_chiseled_stone_bricks",
			"stone_bricks",
			"mossy_stone_bricks",
			"cracked_stone_bricks",
			"chiseled_stone_bricks",
			"brown_mushroom_block",
			"red_mushroom_block",
			"mushroom_stem",
			"iron_bars",
			"glass_pane",
			"melon",
			"vine",
			"oak_fence_gate",
			"spruce_fence_gate",
			"birch_fence_gate",
			"jungle_fence_gate",
			"acacia_fence_gate",
			"dark_oak_fence_gate",
			"brick_stairs",
			"stone_brick_stairs",
			"mycelium",
			"lily_pad",
			"nether_bricks",
			"nether_brick_fence",
			"nether_brick_stairs",
			"enchanting_table",
			"end_portal_frame",
			"end_stone",
			"end_stone_bricks",
			"dragon_egg",
			"redstone_lamp",
			"sandstone_stairs",
			"emerald_ore",
			"ender_chest",
			"tripwire_hook",
			"emerald_block",
			"spruce_stairs",
			"birch_stairs",
			"jungle_stairs",
			"command_block",
			"beacon",
			"cobblestone_wall",
			"mossy_cobblestone_wall",
			"oak_button",
			"spruce_button",
			"birch_button",
			"jungle_button",
			"acacia_button",
			"dark_oak_button",
			"anvil",
			"chipped_anvil",
			"damaged_anvil",
			"trapped_chest",
			"light_weighted_pressure_plate",
			"heavy_weighted_pressure_plate",
			"daylight_detector",
			"redstone_block",
			"nether_quartz_ore",
			"hopper",
			"chiseled_quartz_block",
			"quartz_block",
			"quartz_pillar",
			"quartz_stairs",
			"activator_rail",
			"dropper",
			"white_terracotta",
			"orange_terracotta",
			"magenta_terracotta",
			"light_blue_terracotta",
			"yellow_terracotta",
			"lime_terracotta",
			"pink_terracotta",
			"gray_terracotta",
			"light_gray_terracotta",
			"cyan_terracotta",
			"purple_terracotta",
			"blue_terracotta",
			"brown_terracotta",
			"green_terracotta",
			"red_terracotta",
			"black_terracotta",
			"barrier",
			"iron_trapdoor",
			"hay_block",
			"white_carpet",
			"orange_carpet",
			"magenta_carpet",
			"light_blue_carpet",
			"yellow_carpet",
			"lime_carpet",
			"pink_carpet",
			"gray_carpet",
			"light_gray_carpet",
			"cyan_carpet",
			"purple_carpet",
			"blue_carpet",
			"brown_carpet",
			"green_carpet",
			"red_carpet",
			"black_carpet",
			"terracotta",
			"coal_block",
			"packed_ice",
			"acacia_stairs",
			"dark_oak_stairs",
			"slime_block",
			"grass_path",
			"sunflower",
			"lilac",
			"rose_bush",
			"peony",
			"tall_grass",
			"large_fern",
			"white_stained_glass",
			"orange_stained_glass",
			"magenta_stained_glass",
			"light_blue_stained_glass",
			"yellow_stained_glass",
			"lime_stained_glass",
			"pink_stained_glass",
			"gray_stained_glass",
			"light_gray_stained_glass",
			"cyan_stained_glass",
			"purple_stained_glass",
			"blue_stained_glass",
			"brown_stained_glass",
			"green_stained_glass",
			"red_stained_glass",
			"black_stained_glass",
			"white_stained_glass_pane",
			"orange_stained_glass_pane",
			"magenta_stained_glass_pane",
			"light_blue_stained_glass_pane",
			"yellow_stained_glass_pane",
			"lime_stained_glass_pane",
			"pink_stained_glass_pane",
			"gray_stained_glass_pane",
			"light_gray_stained_glass_pane",
			"cyan_stained_glass_pane",
			"purple_stained_glass_pane",
			"blue_stained_glass_pane",
			"brown_stained_glass_pane",
			"green_stained_glass_pane",
			"red_stained_glass_pane",
			"black_stained_glass_pane",
			"prismarine",
			"prismarine_bricks",
			"dark_prismarine",
			"prismarine_stairs",
			"prismarine_brick_stairs",
			"dark_prismarine_stairs",
			"sea_lantern",
			"red_sandstone",
			"chiseled_red_sandstone",
			"cut_red_sandstone",
			"red_sandstone_stairs",
			"repeating_command_block",
			"chain_command_block",
			"magma_block",
			"nether_wart_block",
			"red_nether_bricks",
			"bone_block",
			"structure_void",
			"observer",
			"shulker_box",
			"white_shulker_box",
			"orange_shulker_box",
			"magenta_shulker_box",
			"light_blue_shulker_box",
			"yellow_shulker_box",
			"lime_shulker_box",
			"pink_shulker_box",
			"gray_shulker_box",
			"light_gray_shulker_box",
			"cyan_shulker_box",
			"purple_shulker_box",
			"blue_shulker_box",
			"brown_shulker_box",
			"green_shulker_box",
			"red_shulker_box",
			"black_shulker_box",
			"white_glazed_terracotta",
			"orange_glazed_terracotta",
			"magenta_glazed_terracotta",
			"light_blue_glazed_terracotta",
			"yellow_glazed_terracotta",
			"lime_glazed_terracotta",
			"pink_glazed_terracotta",
			"gray_glazed_terracotta",
			"light_gray_glazed_terracotta",
			"cyan_glazed_terracotta",
			"purple_glazed_terracotta",
			"blue_glazed_terracotta",
			"brown_glazed_terracotta",
			"green_glazed_terracotta",
			"red_glazed_terracotta",
			"black_glazed_terracotta",
			"white_concrete",
			"orange_concrete",
			"magenta_concrete",
			"light_blue_concrete",
			"yellow_concrete",
			"lime_concrete",
			"pink_concrete",
			"gray_concrete",
			"light_gray_concrete",
			"cyan_concrete",
			"purple_concrete",
			"blue_concrete",
			"brown_concrete",
			"green_concrete",
			"red_concrete",
			"black_concrete",
			"white_concrete_powder",
			"orange_concrete_powder",
			"magenta_concrete_powder",
			"light_blue_concrete_powder",
			"yellow_concrete_powder",
			"lime_concrete_powder",
			"pink_concrete_powder",
			"gray_concrete_powder",
			"light_gray_concrete_powder",
			"cyan_concrete_powder",
			"purple_concrete_powder",
			"blue_concrete_powder",
			"brown_concrete_powder",
			"green_concrete_powder",
			"red_concrete_powder",
			"black_concrete_powder",
			"turtle_egg",
			"dead_tube_coral_block",
			"dead_brain_coral_block",
			"dead_bubble_coral_block",
			"dead_fire_coral_block",
			"dead_horn_coral_block",
			"tube_coral_block",
			"brain_coral_block",
			"bubble_coral_block",
			"fire_coral_block",
			"horn_coral_block",
			"tube_coral",
			"brain_coral",
			"bubble_coral",
			"fire_coral",
			"horn_coral",
			"dead_brain_coral",
			"dead_bubble_coral",
			"dead_fire_coral",
			"dead_horn_coral",
			"dead_tube_coral",
			"tube_coral_fan",
			"brain_coral_fan",
			"bubble_coral_fan",
			"fire_coral_fan",
			"horn_coral_fan",
			"dead_tube_coral_fan",
			"dead_brain_coral_fan",
			"dead_bubble_coral_fan",
			"dead_fire_coral_fan",
			"dead_horn_coral_fan",
			"blue_ice",
			"conduit",
			"iron_door",
			"oak_door",
			"spruce_door",
			"birch_door",
			"jungle_door",
			"acacia_door",
			"dark_oak_door",
			"repeater",
			"comparator",
			"structure_block",
			"smooth_stone_slab",
			"cut_sandstone_slab",
			"cut_red_sandstone_slab",
			"brick_wall",
			"prismarine_wall",
			"red_sandstone_wall",
			"mossy_stone_brick_wall",
			"granite_wall",
			"stone_brick_wall",
			"nether_brick_wall",
			"andesite_wall",
			"red_nether_brick_wall",
			"sandstone_wall",
			"end_stone_brick_wall",
			"diorite_wall",
			"polished_granite_stairs",
			"smooth_red_sandstone_stairs",
			"mossy_stone_brick_stairs",
			"polished_diorite_stairs",
			"mossy_cobblestone_stairs",
			"end_stone_brick_stairs",
			"stone_stairs",
			"smooth_sandstone_stairs",
			"smooth_quartz_stairs",
			"granite_stairs",
			"andesite_stairs",
			"red_nether_brick_stairs",
			"polished_andesite_stairs",
			"diorite_stairs",
			"polished_granite_slab",
			"smooth_red_sandstone_slab",
			"mossy_stone_brick_slab",
			"polished_diorite_slab",
			"mossy_cobblestone_slab",
			"end_stone_brick_slab",
			"smooth_sandstone_slab",
			"smooth_quartz_slab",
			"granite_slab",
			"andesite_slab",
			"red_nether_brick_slab",
			"polished_andesite_slab",
			"diorite_slab",
			"scaffolding",
			"jigsaw",
			"composter",
			"oak_sign",
			"spruce_sign",
			"birch_sign",
			"jungle_sign",
			"acacia_sign",
			"dark_oak_sign",
			"barrel",
			"smoker",
			"blast_furnace",
			"cartography_table",
			"fletching_table",
			"grindstone",
			"lectern",
			"smithing_table",
			"stonecutter",
			"bell",
			"lantern",
			"potted_bamboo",
			"sweet_berry_bush",
			"crimson_nylium",
			"warped_nylium",
			"crimson_planks",
			"warped_planks",
			"nether_gold_ore",
			"crimson_stem",
			"warped_stem",
			"stripped_crimson_stem",
			"stripped_warped_stem",
			"stripped_crimson_hyphae",
			"stripped_warped_hyphae",
			"crimson_hyphae",
			"warped_hyphae",
			"crimson_fungus",
			"warped_fungus",
			"crimson_roots",
			"warped_roots",
			"nether_sprouts",
			"weeping_vines",
			"twisting_vines",
			"crimson_slab",
			"warped_slab",
			"crimson_pressure_plate",
			"warped_pressure_plate",
			"polished_blackstone_pressure_plate",
			"crimson_fence",
			"warped_fence",
			"soul_soil",
			"basalt",
			"polished_basalt",
			"soul_torch",
			"crimson_trapdoor",
			"warped_trapdoor",
			"chain",
			"crimson_fence_gate",
			"warped_fence_gate",
			"cracked_nether_bricks",
			"chiseled_nether_bricks",
			"crimson_stairs",
			"warped_stairs",
			"blackstone_wall",
			"polished_blackstone_wall",
			"polished_blackstone_brick_wall",
			"crimson_button",
			"warped_button",
			"polished_blackstone_button",
			"quartz_bricks",
			"warped_wart_block",
			"crimson_door",
			"warped_door"
		};

		public static Dictionary<NamespacedID, BlockID> allBlocks;

		public static Dictionary<NumericID, BlockID> blockIdByNumerics;
		public static Dictionary<BlockID, string> preFlatteningIDs;

		public static Dictionary<NamespacedID, Remapping> oldRemappings;
		public static Dictionary<BlockID, Remapping> newRemappings;

		public static void Initialize(string blockData, string remappingsData)
		{
			allBlocks = new Dictionary<NamespacedID, BlockID>();
			blockIdByNumerics = new Dictionary<NumericID, BlockID>();
			preFlatteningIDs = new Dictionary<BlockID, string>();
			var lines = blockData.Replace("\r", "").Split('\n');
			//ID,Properties,Numeric ID,Pre-flattening ID,Added in Version,Fallback
			List<(BlockID, string)> fallbacks = new List<(BlockID, string)>();
			for(int i = 2; i < lines.Length; i++)
			{
				var split = lines[i].Split(';');
				if(split[0].Length > 0)
				{
					var id = new NamespacedID(split[0]);
					if(allBlocks.ContainsKey(id))
					{
						//TODO: how to handle multiple equal ids (such as different facing logs) ?
						continue;
					}
					string props = split[1]; //TODO: introduce default properties?
					NumericID? numeric = NumericID.TryParse(split[2]);
					string preFlattening = split[3];
					GameVersion version = split[4].Length > 1 ? GameVersion.Parse(split[4]) : GameVersion.FirstVersion;
					var newBlocks = BlockID.RegisterNewVanillaBlock(id.id, version, null, numeric);
					foreach(var newBlock in newBlocks)
					{
						if(split[5].Length > 1)
						{
							fallbacks.Add((newBlock, split[5]));
						}
						if(numeric.HasValue)
						{
							if(!blockIdByNumerics.ContainsKey(numeric.Value))
							{
								blockIdByNumerics.Add(numeric.Value, newBlock);
							}
						}
						if(preFlattening.Length > 1) preFlatteningIDs.Add(newBlock, "minecraft:" + preFlattening);
					}
				}
			}

			//Set remappings
			oldRemappings = new Dictionary<NamespacedID, Remapping>();
			newRemappings = new Dictionary<BlockID, Remapping>();
			if(remappingsData != null)
			{
				CSV remappingsCSV = new CSV(remappingsData);
				foreach(var line in remappingsCSV.data)
				{
					var newID = Find(new NamespacedID(line[1]), true);
					var oldID = new BlockID(new NamespacedID(line[0]), newID.AddedInVersion, (BlockID)newID.substitute, newID.numericID);
					GameVersion version = GameVersion.Parse(line[2]);
					var remap = new Remapping(oldID, newID, version);
					oldRemappings.Add(oldID.ID, remap);
					newRemappings.Add(newID, remap);
				}
			}

			//Find & set substitute blocks
			foreach(var f in fallbacks)
			{
				//HACK: property parsing not yet implemented.
				string subst = f.Item2;
				subst = subst.Split('[')[0];
				if(subst.Contains("*"))
				{
					subst.Replace("*", GetColorFromBlockID(f.Item1.ID.id));
				}
				BlockID subsituteBlock;
				if(!char.IsDigit(subst[0]))
				{
					subsituteBlock = Find(new NamespacedID(subst));
				}
				else
				{
					subsituteBlock = FindByNumeric(NumericID.Parse(subst));
				}
				f.Item1.substitute = subsituteBlock;
			}
		}

		public static string GetColorFromBlockID(string blockID)
		{
			foreach(var col in Blocks.commonColors)
			{
				if(blockID.StartsWith(col + "_")) return col;
			}
			return null;
		}

		public static BlockID Find(NamespacedID id, bool throwErrorIfNotFound = false)
		{
			if(!id.HasCustomNamespace)
			{
				if(oldRemappings.TryGetValue(id, out var remap))
				{
					return remap.newID;
				}
				if(allBlocks.TryGetValue(id, out var b))
				{
					return b;
				}
				else
				{
					if(throwErrorIfNotFound) throw new KeyNotFoundException($"Unable to find a block with id '{id}'.");
					return null;
				}
			}
			else
			{
				//Modded block, add it to the list if we haven't done so already.
				if(allBlocks.TryGetValue(id, out var b))
				{
					return b;
				}
				else
				{
					return BlockID.RegisterNewModBlock(id.customNamespace, id.id)[0];
				}
			}
		}

		public static BlockID Find(string id, bool throwErrorIfNotFound = false)
		{
			return Find(new NamespacedID(id), throwErrorIfNotFound);
		}

		public static bool TryGetPreviousID(BlockID id, GameVersion targetVersion, out BlockID previous)
		{
			if(newRemappings.TryGetValue(id, out var remap))
			{
				if(targetVersion < remap.version)
				{
					previous = remap.oldID;
					return true;
				}
			}
			previous = null;
			return false;
		}

		//TODO: return proper BlockState by metadata
		public static BlockID FindByNumeric(NumericID numeric, bool throwErrorIfNotFound = false)
		{
			if(blockIdByNumerics.TryGetValue(numeric, out var block))
			{
				return block;
			}
			else if(blockIdByNumerics.TryGetValue(numeric.WithoutDamage, out block))
			{
				return block;
			}
			else
			{
				if(throwErrorIfNotFound) throw new KeyNotFoundException($"Unable to find block definition with numeric ID '{numeric}'.");
				return null;
			}
		}
	}
}

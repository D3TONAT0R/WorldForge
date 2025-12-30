using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		public static ConcurrentDictionary<NamespacedID, BlockID> allBlocks;

		public static Dictionary<NumericID, BlockID> blockIdByNumerics;
		public static Dictionary<BlockID, string> preFlatteningIDs;

		public static Dictionary<NamespacedID, Remapping> oldRemappings;
		public static Dictionary<BlockID, Remapping> newRemappings;

		public static void Initialize(string blockData, string remappingsData)
		{
			Logger.Verbose("Initializing block list ...");
			allBlocks = new ConcurrentDictionary<NamespacedID, BlockID>();
			blockIdByNumerics = new Dictionary<NumericID, BlockID>();
			preFlatteningIDs = new Dictionary<BlockID, string>();
			oldRemappings = new Dictionary<NamespacedID, Remapping>();
			newRemappings = new Dictionary<BlockID, Remapping>();

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
					//Future vanilla block, add it as a new block.
					Logger.Warning("Future block ID encountered: " + id);
					var block = BlockID.RegisterNewVanillaBlock(id.id, GameVersion.FirstVersion)[0];
					return block;
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

		public static BlockID[] Search(string searchTerm, bool includeNonVanilla = false, bool throwIfNoResults = false)
		{
			List<BlockID> results = new List<BlockID>();
			//searchTerm = "^" + searchTerm.Replace("*", @"\**") + "$";
			searchTerm = "^" + searchTerm.Replace("*", @"\**");
			foreach(var b in allBlocks.Keys)
			{
				if(b.HasCustomNamespace && !includeNonVanilla) continue;
				if(Regex.IsMatch(b.id, searchTerm))
				{
					results.Add(allBlocks[b]);
				}
			}
			if(results.Count == 0 && throwIfNoResults) throw new ArgumentException($"Block ID search returned no results: {searchTerm}");
			return results.ToArray();
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

		internal static void Register(BlockID b)
		{
			allBlocks.TryAdd(b.ID, b);
		}
	}
}

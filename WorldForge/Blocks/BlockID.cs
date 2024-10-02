using WorldForge.Items;

namespace WorldForge
{
	public class BlockID : ItemID
	{
		public bool IsVanillaBlock => customNamespace == null || customNamespace == "minecraft";
		public bool IsAir => ID == "minecraft:air";
		public bool IsWater => ID == "minecraft:water";
		public bool IsLava => ID == "minecraft:lava";
		public bool IsLiquid => IsWater || IsLava;

		/// <summary>
		/// Registers a new vanilla block type.
		/// </summary>
		public static BlockID[] RegisterNewVanillaBlock(string shortID, GameVersion versionAdded, BlockID substitute = null, NumericID? numID = null)
		{
			return RegisterNewBlock(null, shortID, versionAdded, substitute, numID);
		}

		/// <summary>
		/// Registers a new modded block type (does not check for game versions).
		/// </summary>
		public static BlockID[] RegisterNewModBlock(string modNamespace, string shortID, NumericID? numID = null)
		{
			return RegisterNewBlock(modNamespace, shortID, null, null, numID);
		}

		static BlockID[] RegisterNewBlock(string modNamespace, string shortID, GameVersion? versionAdded, BlockID substitute, NumericID? numericId)
		{
			if(!versionAdded.HasValue) versionAdded = GameVersion.FirstVersion;
			if(shortID.Contains("*"))
			{
				//Multicolored blocks
				BlockID[] blocks = new BlockID[Blocks.commonColors.Length];
				for(int i = 0; i < Blocks.commonColors.Length; i++)
				{
					string colorBlockID = shortID.Replace("*", Blocks.commonColors[i]);
					var b = new BlockID(modNamespace, colorBlockID, versionAdded.Value, substitute, numericId);
					BlockList.allBlocks.Add(b.ID, b);
					blocks[i] = b;
				}
				return blocks;
			}
			else
			{
				var b = new BlockID(modNamespace, shortID, versionAdded.Value, substitute, numericId);
				BlockList.allBlocks.Add(b.ID, b);
				return new BlockID[] { b };
			}
		}

		public BlockID(string ns, string id, GameVersion v, BlockID sub, NumericID? num) : base(ns, id, v, sub, num)
		{

		}

		/// <summary>
		/// Returns the most appropriate block by game version. If this block does not yet exist in the given version, it will recursively search for a replacement. If there is none available, null (air) is returned.
		/// </summary>
		public BlockID FindAppropriateBlock(GameVersion gameVersion)
		{
			if(gameVersion >= AddedInVersion)
			{
				return this;
			}
			else
			{
				if(substitute != null)
				{
					return ((BlockID)substitute).FindAppropriateBlock(gameVersion);
				}
				else
				{
					return null;
				}
			}
		}

		public bool CompareMultiple(params string[] ids)
		{
			bool b = false;
			for(int i = 0; i < ids.Length; i++)
			{
				b |= Compare(ids[i]);
			}
			return b;
		}

		public bool Compare(string block)
		{
			return block == ID;
		}

		public override bool Equals(object obj)
		{
			if(obj is BlockID pb)
			{
				return this == pb;
			}
			else if(obj is string s)
			{
				return ID == s;
			}
			else
			{
				return false;
			}
		}

		public static bool operator ==(BlockID l, BlockID r)
		{
			if(l is null && r is null) return true;
			else if(l is null || r is null) return false;
			else return l.ID == r.ID;
		}

		public static bool operator !=(BlockID l, BlockID r)
		{
			return !(l == r);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = shortID.GetHashCode();
				if(customNamespace != null)
				{
					hash += customNamespace.GetHashCode();
				}
				return hash;
			}
		}

		public override string ToString()
		{
			return ID;
		}
	}
}

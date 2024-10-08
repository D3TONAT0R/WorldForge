using WorldForge.Items;

namespace WorldForge
{
	public class BlockID : ItemID
	{
		public override int GetHashCode()
		{
			throw new System.NotImplementedException();
		}

		private static NamespacedID air = new NamespacedID("air", false);
		private static NamespacedID water = new NamespacedID("water", false);
		private static NamespacedID lava = new NamespacedID("lava", false);

		public bool IsVanillaBlock => !ID.HasCustomNamespace;
		public bool IsAir => ID == air;
		public bool IsWater => ID == water;
		public bool IsLava => ID == lava;
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

		private static BlockID[] RegisterNewBlock(string modNamespace, string id, GameVersion? versionAdded, BlockID substitute, NumericID? numericId)
		{
			if(!versionAdded.HasValue) versionAdded = GameVersion.FirstVersion;
			if(id.Contains("*"))
			{
				//Multicolored blocks
				BlockID[] blocks = new BlockID[Blocks.commonColors.Length];
				for(int i = 0; i < Blocks.commonColors.Length; i++)
				{
					string colorBlockID = id.Replace("*", Blocks.commonColors[i]);
					var b = new BlockID(new NamespacedID(modNamespace, id), versionAdded.Value, substitute, numericId);
					BlockList.allBlocks.Add(b.ID, b);
					blocks[i] = b;
				}
				return blocks;
			}
			else
			{
				var b = new BlockID(new NamespacedID(modNamespace, id), versionAdded.Value, substitute, numericId);
				BlockList.allBlocks.Add(b.ID, b);
				return new BlockID[] { b };
			}
		}

		public BlockID(NamespacedID id, GameVersion v, BlockID sub, NumericID? num) : base(id, v, sub, num)
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

		public bool CompareMultiple(string[] ids)
		{
			for(int i = 0; i < ids.Length; i++)
			{
				if(Compare(ids[i])) return true;
			}
			return false;
		}

		public bool CompareMultiple(string b0, string b1)
		{
			return Compare(b0) || Compare(b1);
		}

		public bool Compare(string block)
		{
			return ID.Matches(block);
		}

		public override bool Equals(object obj)
		{
			if(obj is BlockID pb)
			{
				return this == pb;
			}
			else if(obj is string s)
			{
				return ID.Matches(s);
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

		protected bool Equals(BlockID other)
		{
			return this == other;
		}
	}
}

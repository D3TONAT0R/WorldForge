using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils
{
	public class ProtoBlock
	{
		public string ID => (customNamespace ?? "minecraft") + ":" + shortID;

		public readonly string customNamespace = null;
		public readonly string shortID;

		public readonly Version addedInVersion;
		public ProtoBlock substitute;

		public bool IsAir => ID == "minecraft:air";
		public bool IsWater => ID == "minecraft:water";
		public bool IsLava => ID == "minecraft:lava";
		public bool IsLiquid => IsWater || IsLava;

		/// <summary>
		/// Registers a new vanilla block type.
		/// </summary>
		public static ProtoBlock[] RegisterNewVanillaBlock(string shortID, Version versionAdded, ProtoBlock substitute = null)
		{
			return RegisterNewBlock(null, shortID, versionAdded, substitute);
		}

		/// <summary>
		/// Registers a new modded block type (does not check for game versions).
		/// </summary>
		public static ProtoBlock[] RegisterNewModBlock(string modNamespace, string shortID)
		{
			return RegisterNewBlock(modNamespace, shortID, null, null);
		}

		static ProtoBlock[] RegisterNewBlock(string modNamespace, string shortID, Version? versionAdded, ProtoBlock substitute)
		{
			if(!versionAdded.HasValue) versionAdded = Version.FirstVersion;
			if(shortID.Contains("*"))
			{
				//Multicolored blocks
				ProtoBlock[] blocks = new ProtoBlock[Blocks.commonColors.Length];
				for (int i = 0; i < Blocks.commonColors.Length; i++)
				{
					string colorBlockID = shortID.Replace("*", Blocks.commonColors[i]);
					//TODO: how to substitute color block with another color block?
					var b = new ProtoBlock(modNamespace, colorBlockID, versionAdded.Value, substitute);
					BlockList.allBlocks.Add(b.ID, b);
					blocks[i] = b;
				}
				return blocks;
			}
			else
			{
				var b = new ProtoBlock(modNamespace, shortID, versionAdded.Value, substitute);
				BlockList.allBlocks.Add(b.ID, b);
				return new ProtoBlock[] { b };
			}
		}

		private ProtoBlock(string ns, string id, Version v, ProtoBlock sub)
		{
			customNamespace = ns;
			shortID = id;
			addedInVersion = v;
			substitute = sub;
		}

		/// <summary>
		/// Returns the most appropriate block by game version. If this block does not yet exist in the given version, it will recursively search for a replacement. If there is none available, null (air) is returned.
		/// </summary>
		public ProtoBlock FindAppropriateBlock(Version gameVersion)
		{
			if(gameVersion >= addedInVersion)
			{
				return this;
			}
			else
			{
				if(substitute != null)
				{
					return substitute.FindAppropriateBlock(gameVersion);
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
			for (int i = 0; i < ids.Length; i++)
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
			if(obj is ProtoBlock pb)
			{
				return this == pb;
			} else if(obj is string s)
			{
				return ID == s;
			}
			else
			{
				return false;
			}
		}

		public static bool operator ==(ProtoBlock l, ProtoBlock r)
		{
			if (l is null && r is null) return true;
			else if (l is null || r is null) return false;
			else return l.ID == r.ID;
		}

		public static bool operator !=(ProtoBlock l, ProtoBlock r)
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
